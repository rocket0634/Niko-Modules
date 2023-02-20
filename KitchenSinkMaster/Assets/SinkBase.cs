using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

abstract partial class SinkBase : MonoBehaviour {
#region Fields
#pragma warning disable 649    // Stop compiler warning: “Field is not assigned to” (they are assigned in Unity)
	public KMBombModule Module;
	// Multiple Bombs will throw an exception if the Scaffold is not included in Mod.Bundle and has a KMBombInfo component on it.
	// Put the KMBombInfo component on the loaded object instead so it works correctly
	public KMBombInfo Info;
	// The game does not find the Scaffold object when it is not included in Mod.Bundle, so KMAudio must be included with the module prefabs.
	public KMAudio Audio;
	// The prefab that holds components shared between all child modules.
    public SinkScaffold ScaffoldPrefab;
	// Used to set the selectables to the copied prefab.
	public KMSelectable mainSelectable;
#pragma warning restore 649
	// Copy of the prefab used by the game.
	protected SinkScaffold _scaffold;
	// Identifies the mod for logging purposes. This variable is set exclusively in each module.
	protected int _moduleId;
	protected KMSelectable Cold { get { return _scaffold.Cold; } }
	protected KMSelectable Hot { get { return _scaffold.Hot; } }
	// Used to set the selectables to the copied prefab. This variable is set exclusively in each module.
	protected KMSelectable[] _selectableChildren;
	protected int ColKnobs, ColFaucet, ColPipe, curKnob, batteries;
	/* 
	 * canPress - Used to determine when the button queue can continue after module activation.
	 * SOLVED - Used to disable button inputs after the module is disarmed.
	 */
	protected bool canPress, SOLVED;
	// Contains the conditions used to set up the module.
	protected bool[] Rules;
	// Contains the expected condition for each stage, held in a class that contains the selectable and the expected boolean value.
	protected Temp[] knob2Turn;
	private readonly string[] textureList = { "Copper", "Stainless Steel", "Gold-Plated" };
	protected readonly int[][] selectableRules = { new[] { 1, 0, 3 }, new[] { 2, 5, 1 }, new[] { 4, 2, 0 }, new[] { 4, 5, 3 } };
	protected int[] selectedRules;
	private Queue<IEnumerable> queue = new Queue<IEnumerable>();
	protected static readonly string SinkHelpMessage = "Interact with the module by using !{0} Hot or !{0} Cold. You may chain commands by using !{0} cch or !{0} cold cold hot.";
	protected MeshRenderer HotMesh { get { return _scaffold.HotMesh; }}
	protected MeshRenderer ColdMesh { get { return _scaffold.ColdMesh; }}
	protected MeshRenderer FaucetMesh { get { return _scaffold.FaucetMesh; }}
	protected MeshRenderer PipeMesh { get { return _scaffold.PipeMesh; }}
	protected Dictionary<KMSelectable, Temp> knobs = new Dictionary<KMSelectable, Temp>();
	protected Dictionary<KMSelectable, KMSelectable.OnInteractHandler> Handlers = new Dictionary<KMSelectable, KMSelectable.OnInteractHandler>();
#endregion

#region Pre-Init
#pragma warning disable 660, 661
	protected class Temp
	{
		internal KMSelectable knob;
		internal bool temp;
		internal string name { get { return knob.name; } }
		// Handles rotation potition of the Hot and Cold knobs.
		internal float p;
		public Temp(KMSelectable selectable = null)
		{
			knob = selectable;
		}
	}
#pragma warning restore 660, 661
	protected void Awake()
	{
		_scaffold = Instantiate(ScaffoldPrefab, transform);
		_scaffold.gameObject.SetActive(true);
		SetFields();
		Log("KitchenSink - v{0}", _scaffold.ModSource.Version());
		UpdateChildren();
		knobs.Add(Cold, new Temp(Cold));
		knobs.Add(Hot, new Temp(Hot){ temp = true });
		Handlers.Add(Cold, () => ButtonHandler(Cold));
		Handlers.Add(Hot, () => ButtonHandler(Hot));
	}
	// Used to set _moduleId and _selectableChildren
	protected abstract void SetFields();

    protected void UpdateChildren()
    {
		if (mainSelectable.Children.Length < _selectableChildren.Length)
			mainSelectable.Children = new KMSelectable[_selectableChildren.Length];
		for (int i = 0; i < _selectableChildren.Length; i++)
		{
			mainSelectable.Children[i] = _selectableChildren[i];
			if (_selectableChildren[i] != null)
				_selectableChildren[i].Parent = mainSelectable;
		}
        mainSelectable.UpdateChildrenProperly();
    }
#endregion

#region Init
	protected void Start()
	{
		ColKnobs = UnityEngine.Random.Range(0, 3);
		ColFaucet = UnityEngine.Random.Range(0, 3);
		ColPipe = UnityEngine.Random.Range(0, 3);
		Rules = new[] { Info.GetOffIndicators().Contains("NSA"), 
			Info.GetSerialNumberLetters().Any("AEIOU".Contains),
			ColKnobs == 2, ColFaucet == 1, ColPipe == 0,
			Info.GetPorts().Contains("HDMI") || Info.GetPorts().Contains("RJ45")};
		Steps();
		ApplyTextures(Enumerable.Range(0, 4));
		Module.OnActivate += () => canPress = true;
	}
	protected virtual void ApplyTextures(IEnumerable<int> nums)
	{
		var renderers = new[] { ColdMesh, HotMesh, FaucetMesh, PipeMesh };
		var partColors = new[] { ColKnobs, ColKnobs, ColFaucet, ColPipe };
		foreach (int i in nums)
			renderers[i].material.mainTexture = _scaffold.KnobColors[partColors[i]];
		if (ColPipe == 2)
			PipeMesh.material.mainTexture = null;
	}

	protected virtual void Steps()
	{
		batteries = Info.GetBatteryCount() / 2;
		if (batteries.InRange(1, 2))
			Rules = Rules.Select(x => !x).ToArray();
		// If there are more than 6 batteries, we clamp it to 6 so that we don't get an IndexOutOfRangeException
		selectedRules = selectableRules[Mathf.Clamp(batteries, 0, 3)];
		var colorList = textureList.ToArray();
		colorList[2] = "PVC";
		var assign = new Action[] { () => StartCoroutine(CheckForTurn()),
		() => Cold.OnInteract += Handlers[Cold],
		() => Hot.OnInteract += Handlers[Hot],
		// Determine which knob to turn for all three stages
		() => knob2Turn = selectedRules.Select(x => Rules[x] ? knobs[Hot] : knobs[Cold]).ToArray(),
		() => Log("Knobs are {0}, Faucet is {1}, Drain Pipe is {2}", textureList[ColKnobs], textureList[ColFaucet], colorList[ColPipe]),
		() => Log("First Knob: {0}", knob2Turn[0].name),
		() => Log("Second Knob: {0}", knob2Turn[1].name),
		() => Log("Third Knob: {0}", knob2Turn[2].name) };
		DoAssigns(assign);
	}

	protected virtual void DoAssigns(Action[] assign)
	{
		foreach (Action action in assign)
			action();
	}

	protected bool ButtonHandler(KMSelectable temp)
	{
		queue.Enqueue(ButtonPress(knobs[temp]));
		return false;
	}
#endregion

#region Coroutines
	protected IEnumerator CheckForTurn()
	{
		while (isActiveAndEnabled)
		{
			yield return new WaitUntil(() => canPress);
			if (queue.Count > 0)
			{
				IEnumerable press = queue.Dequeue();
				foreach (object item in press) yield return item;
			}
		}
	}

	protected virtual IEnumerable ButtonPress(Temp t)
	{
		var knob = t.knob;
		knob.AddInteractionPunch(0.5f);
		Audio.PlaySoundAtTransform("valve_spin", knob.transform);
		if (!SOLVED)
		{
			canPress = false;
			if (t == knob2Turn[curKnob])
			{
				yield return KnobTurn(t, 2.5f);
				if (curKnob == 2)
					Solve();
				curKnob++;
			}
			else
			{
				yield return KnobTurn(t, -2.5f);
				yield return ResetTurn();
				knobs[Hot].p = 0;
				knobs[Cold].p = 0;
				Module.HandleStrike();
				curKnob = 0;
			}
			canPress = true;
		}
	}

	protected IEnumerator KnobTurn(Temp r, float turn)
	{
		var hold = 0;
		while (hold != 15)
		{
			yield return new WaitForSeconds(0.001f);
			r.knob.transform.Rotate(Vector3.up, turn);
			hold++;
		}
		r.p += turn;
	}

	private IEnumerator ResetTurn()
	{
		var hold = 0;
		while (hold != 15)
		{
			yield return new WaitForSeconds(0.001f);
			Cold.transform.Rotate(Vector3.up, -knobs[Cold].p);
			Hot.transform.Rotate(Vector3.up, -knobs[Hot].p);
			hold++;
		}
	}
#endregion

#region Twitch Plays
	protected string matches = "ch";
	protected string fullMatch = "^cold|hot$";
	protected int[] indicies = new[] { 0, 1 };
	private List<KMSelectable> DetermineCharMatch(string[] input)
	{
		var q = new List<KMSelectable>();
		foreach (string s in input)
		{
			var Match2 = s.RegexMatch(fullMatch);
			var newStr = Match2 ? s.Substring(0, 1) : s;
			var Match = newStr.RegexMatch(string.Format("^[{0}]+$", matches));
			if (Match)
				foreach (char c in newStr)
				{
					var i = matches.IndexOf(c);
					if (i > -1 && _selectableChildren[indicies[i]] != null)
					q.Add(_selectableChildren[indicies[i]]);
				}
		}
		return q;
	}

	protected virtual IEnumerator ProcessTwitchCommand(string tpInput)
	{
		string[] input = tpInput.ToLowerInvariant().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
		KMSelectable[] tapList = DetermineCharMatch(input).ToArray();
		yield return null;
		yield return tapList;
	}

	protected virtual IEnumerator TwitchHandleForcedSolve()
	{
		for (int i = curKnob; i < knob2Turn.Length; i++)
		{
			knob2Turn[i].knob.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
	}
#endregion

	protected void Solve()
	{
		StopAllCoroutines();
		SOLVED = true;
		Module.HandlePass();
		Log("The module has been disarmed.");
	}

	protected void Log(string msg, params object[] args)
	{
		var format = string.Format(msg, args);
		Debug.LogFormat("[{0} #{1}] {2}", Module.ModuleDisplayName, _moduleId, format);
	}
}