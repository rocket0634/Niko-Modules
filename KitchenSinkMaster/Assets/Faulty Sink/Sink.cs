using KMBombInfoExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Sink : MonoBehaviour
{
    #region Fields
    public enum Type
    {
        Normal,
        Faulty
    }
    private static int sink_moduleIDCounter = 1, fSink_moduleIDCounter = 1;
    private int moduleID;

    public KMBombInfo Info;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable Cold, Hot, Faucet, Pipe, Basin;
    public MeshRenderer ColdMesh, HotMesh, FaucetMesh, PipeMesh;
    public TextMesh C, H;
    public Texture[] knobColors;
    public Type ModuleType;
    //For Unity, no use in game
    public int forceSpin;
    public float seconds;

    private readonly string[] textureList = { "Copper", "Stainless Steel", "Gold-Plated" },
        colorList = { "Copper", "Stainless Steel", "PVC" };
    /* canPress - used for Twitch Plays and most button interactions with Vanilla Sink and Vanilla Sink functions within Faulty Sink
     * SOLVED - used for Vanilla button pressing. Buttons can no longer be pressed after the module is solved. Also used to let TP know not to spit out errors in chat
     * o - only used in ChooseTexture, so that it isn't accessed when using the main Vanilla functions
     * rotate - Permission to rotate / rotating - A knob is rotating / wait - Used to know rather a button needs to be held or not
     * h - To let Timer() know it should run. [Technically not needed as Timer is stopped in most cases, and nothing else checks for if it's holding]
     * processingInput - Debugging for Twitch Plays. Hopefully users won't see this. / isFaulty - Type is Faulty
     */
    protected bool canPress, SOLVED, o, rotate, rotating, wait, h, processingInput, isFaulty;
    //c - Determine which faulty spin function to use
    private int curKnob, ColKnobs, ColFaucet, ColPipe, c, faulty, spin;
    protected bool[] Rules, knob2Turn = new bool[3];
    private int[] selectedRules;
    //ChooseTexture variable
    private Texture hold;
    private Material buttonMasher;
    //1 - Spinning or KnobTurn / 2 - KnobTurn or Timer / 3 - Updating spin [was previously in Update]
    private IEnumerator coroutine, coroutine2, coroutine3;
    //Handles rotation position of the Hot and Cold knobs. dT - used only for Timer
    protected float coldP, hotP, dT;
    //Used mostly for Vanilla Sink, as most Faulty functions bypass the queue.
    private Queue<IEnumerable> queue = new Queue<IEnumerable>();
    //Due to weird OnInteract/delegate stuff, thought it might be best to put the default InteractHandlers in variables
    private KMSelectable.OnInteractHandler HotHandler, ColdHandler;
    //Selectable to hold Hot when it can't be interacted with
    private KMSelectable hotHolder;
    #endregion

    #region Twitch Plays
    public string TwitchHelpMessage = "";

    private List<KMSelectable> DetermineCharMatch(string input)
    {
        var Match = isFaulty ? Regex.Match(input, "^[chpbsfd]+$") : Regex.Match(input, "^[ch]+$");
        var matches = isFaulty ? new[] { 'c', 'h', 'p', 'b', 's', 'f', 'd' } : new[] { 'c', 'h' };
        var items = isFaulty ? new[] { Cold, Hot, Pipe, Basin, Basin, Faucet, Pipe } : new[] { Cold, Hot };
        var indicies = GetComponent<KMSelectable>().Children.Count() > 2 ? new[] { 0, 2, 7, 3, 3, 1, 7 } : new[] { 0, 1 };
        if (!isFaulty) indicies = indicies.Take(2).ToArray();
        var list = new List<KMSelectable>();
        if (Match.Success)
            foreach (char c in input)
            {
                var i = Array.IndexOf(matches, c);
                //Typing "hot" when everything is black would turn the proper interactable
                //That's cool and all, but not how the module is supposed to function.
                if (i == 1 && (Hot == Faucet || Hot == Pipe))
                {
                    return new List<KMSelectable> { hotHolder };
                }
                //Don't interact with a selectable if it isn't active
                else if (Info.GetComponent<KMSelectable>().Children[indicies[i]] != null)
                    list.Add(items[i]);
                else
                    return null;
            }
        else return null;
        return list;
    }

    private bool HotCheck(KMSelectable selectable)
    {
        if (selectable == hotHolder && (Hot.Equals(Faucet) || Hot.Equals(Pipe)))
        {
            return true;
        }
        return false;
    }

    private IEnumerator ProcessTwitchCommand(string tpInput)
    {
        //When stuck in a while loop...
        if (processingInput && !SOLVED)
        {
            yield return "sendtochaterror Still processing previous input, command dropped";
            yield break;
        }
        string Input = tpInput.ToLowerInvariant();

        if (Input == "sinkcommands")
        {
            yield return "sendtochat Sink: Interact with the module by using !{1} Hot or !{1} Cold. You may chain commands by using !{1} cch or !{1} cold cold hot. Manual: https://ktane.timwi.de/HTML/Sink.html";
            yield break;
        }

        List<KMSelectable> tapList = new List<KMSelectable>();

        tapList = DetermineCharMatch(Input) ?? tapList;
        if (tapList.Count == 0)
        {
            string[] taps = Input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tap in taps)
            {
                var items = new[] { "h", "c", "f", "p", "b" };
                var index = new[] { tap.Equals("hot") || tap.Equals("h"), tap.Equals("cold") || tap.Equals("c"), isFaulty && (tap.Equals("faucet") || tap.Equals("f")),
                isFaulty && (tap.Equals("pipe") || tap.Equals("p") || tap.Equals("drain") || tap.Equals("d") || tap.Equals("dp")),
                isFaulty && (tap.Equals("sink") || tap.Equals("s") || tap.Equals("basin") || tap.Equals("b"))};
                var i = Array.IndexOf(index, true);
                if (i != -1)
                {
                    var selectable = DetermineCharMatch(items[i]);
                    if (selectable == null) yield break;
                    if (HotCheck(selectable[0]))
                    {
                        tapList = selectable;
                        break;
                    }
                    tapList.Add(selectable[0]);
                }
                else yield break;
            }
        }
        if (HotCheck(tapList.First()))
        {
            yield return null;
            queue.Enqueue(ButtonPress(tapList.First()));
            yield break;
        }

        while (tapList.Count != 0)
        {
            var isRotating = rotating;
            processingInput = true;
            yield return null;
            yield return new KMSelectable[] { tapList.First() };
            tapList.RemoveAt(0);
            yield return new WaitUntil(() => canPress || rotating);
            if (rotating && c == 2)
            {
                Skip();
                yield return new WaitUntil(() => !rotating);
            }
            if (isRotating != rotating && tapList.Count > 0)
            {
                yield return "sendtochaterror Due to a sudden change, not all inputs were processed.";
                processingInput = false;
                yield break;
            }
        }
        if (rotating && c == 1) curKnob = 0;
        processingInput = false;
    }
    #endregion

    #region Init
    protected void Start()
    {
        isFaulty = ModuleType == Type.Faulty;
        ColKnobs = UnityEngine.Random.Range(0, 3);
        ColFaucet = UnityEngine.Random.Range(0, 3);
        ColPipe = UnityEngine.Random.Range(0, 3);

        Rules = new bool[6] { Info.GetOffIndicators().Contains("NSA"), Info.GetSerialNumberLetters().Any("AEIOU".Contains), (ColKnobs == 2), (ColFaucet == 1), (ColPipe == 0), Info.GetPorts().Contains("HDMI") || Info.GetPorts().Contains("RJ45") };

        if (!isFaulty)
        {
            moduleID = sink_moduleIDCounter++;
        }
        else
        {
            Module.ModuleDisplayName = "Faulty Sink";
            Module.ModuleType = "Faulty Sink";
            moduleID = fSink_moduleIDCounter++;
            HotHandler = Hot.OnInteract;
            ColdHandler = Cold.OnInteract;
            coroutine3 = Spin();
            StartCoroutine(coroutine3);
            spin = forceSpin + UnityEngine.Random.Range(0, 20);
            forceSpin = spin;
            if (spin > 10)
                rotate = true;
            faulty = UnityEngine.Random.Range(0, 5);
            switch (faulty)
            {
                case 0:
                    PipeMesh.material.color = Color.black;
                    Apply(new[] { 0, 1, 2 });
                    for (int i = 3; i < 6; i++)
                        Module.GetComponent<KMSelectable>().Children[i] = Basin;
                    Basin.OnInteract = delegate () { FaultyPVC(1); return false; };
                    Hot.OnInteract = delegate () { FaultyPVC(0); return false; };
                    Cold.OnInteract = delegate () { FaultyPVC(2); return false; };
                    UpdateChildren();
                    DebugLog("Module status is unknown. Please submit [Hot] [Basin] to automatically disable the module.");
                    goto start;
                case 1:
                    ColPipe = 2;
                    PipeMesh.material.color = new Color(0, 157 / 255f, 1);
                    Rules[4] = false;
                    Rules = Rules.Select(x => !x).ToArray();
                    DebugLog("Debug: PVC reported as blue.");
                    break;
                case 2:
                    var r = UnityEngine.Random.Range(0, 2);
                    GetComponent<KMSelectable>().Children[1] = Faucet;
                    GetComponent<KMSelectable>().Children[7] = Pipe;
                    UpdateChildren();
                    while(new[] { ColFaucet, ColPipe }.All(x => x == ColKnobs) || new[] { ColFaucet, ColPipe }.All(x => x != ColKnobs))
                    {
                        ColFaucet = UnityEngine.Random.Range(0, 3);
                        ColPipe = UnityEngine.Random.Range(0, 3);
                    }
                    Apply(new[] { 0, 1, 2 });
                    PipeMesh.material.mainTexture = knobColors[ColPipe];
                    if (r == 0) ColdMesh.material = null;
                    else HotMesh.material = null;
                    Faucet.OnInteract = ButtonHandler(Faucet, 1, r);
                    Pipe.OnInteract = ButtonHandler(Pipe, 1, r);
                    Cold.OnInteract = ButtonHandler(Cold, 1, r);
                    Hot.OnInteract = ButtonHandler(Hot, 1, r);
                    Rules[3] = ColFaucet.Equals(1);
                    Rules[4] = ColPipe.Equals(0);
                    DebugLog("Debug: Cannot determine status of Button{0}. Please select one of the following textures:", r);
                    DebugLog("Knobs: {0}", knobColors[ColKnobs].name);
                    DebugLog("Faucet: {0}", FaucetMesh.material.mainTexture.name);
                    DebugLog("Drain Pipe: {0}", PipeMesh.material.mainTexture.name);
                    goto start;
                case 3:
                    rotate = false;
                    var s = UnityEngine.Random.Range(0, 2);
                    Hot.OnInteract = delegate () { Module.HandleStrike(); DebugLog("Warning: Overheating"); processingInput = false; return false; };
                    hotHolder = Hot;
                    if (s == 0)
                    {
                        Hot = Faucet;
                        Module.GetComponent<KMSelectable>().Children[1] = Faucet;
                    }
                    else
                    {
                        Hot = Pipe;
                        Module.GetComponent<KMSelectable>().Children[7] = Pipe;
                    }
                    ColKnobs = 0;
                    ColKnobs = 0;
                    ColFaucet = 0;
                    ColPipe = 2;
                    Rules[2] = false;
                    Rules[3] = false;
                    Rules[4] = false;
                    ColdMesh.material.color = Color.black;
                    HotMesh.material.color = Color.black;
                    FaucetMesh.material.color = Color.black;
                    PipeMesh.material.color = Color.black;
                    Steps(false);
                    goto start;
                case 4:
                    transform.Rotate(0, 180, 0);
                    Steps(true);
                    Apply(new[] { 3, 2, 1, 0 });
                    goto start;
            }
        }

        Apply(new[] { 0, 1, 2, 3 });
        Steps(false);

        start: Module.OnActivate += delegate {
            canPress = true;
        };
    }

    private void Apply(int[] nums)
    {
        var apply = new Func<bool>[] { () => { ColdMesh.material.mainTexture = knobColors[ColKnobs]; return false; },
        () => { HotMesh.material.mainTexture = knobColors[ColKnobs]; return false; },
        () => { FaucetMesh.material.mainTexture = knobColors[ColFaucet]; return false; },
        () => { PipeMesh.material.mainTexture = ColPipe != 2 ? knobColors[ColPipe] : null; return false; } };

        foreach (int n in nums)
        {
            apply[n]();
        }
    }

    private void Steps(bool upside)
    {
        int Batteries = (Info.GetBatteryCount() > 6 ? 6 : Info.GetBatteryCount()) / 2;
        var selectableRules = new int[][] { new[] { 1, 0, 3 }, new[] { 2, 5, 1 }, new[] { 4, 2, 0 }, new[] { 4, 5, 3 } };
        if (upside)
        {
            Rules = Rules.Reverse().ToArray();
            for (int i = 0; i < selectableRules.Count(); i++)
                selectableRules[i] = selectableRules[i].Reverse().ToArray();
            selectableRules = selectableRules.Reverse().ToArray();
        }
        if (Batteries > 0 && Batteries < 3)
            Rules = Rules.Select(x => !x).ToArray();
        selectedRules = selectableRules[Batteries];
        var knob = new[] { Rules[selectedRules[0]], Rules[selectedRules[1]], Rules[selectedRules[2]] };

        var reverse = new Func<bool>[] { () => { StartCoroutine(CheckForTurn()); return false; },
        () => { Cold.OnInteract = ButtonHandler(Cold, 0); return false; },
        () => { Hot.OnInteract = ButtonHandler(Hot, 0); return false; },
        () => { knob2Turn[0] = knob[0]; return false; },
        () => { knob2Turn[1] = knob[1]; return false; },
        () => { knob2Turn[2] = knob[2]; return false; },
        () => { DebugLog("Knobs are {0}, Faucet is {1}, Drain Pipe is {2}", textureList[ColKnobs], textureList[ColFaucet], colorList[ColPipe]); return false; },
        () => { DebugLog("First Knob: {0}", (upside ? knob[2] : knob[0]) ? "Hot" : "Cold"); return false; },
        () => { DebugLog("Second Knob: {0}", knob[1] ? "Hot" : "Cold"); return false; },
        () => { DebugLog("Third Knob: {0}", (upside ? knob[0] : knob[2]) ? "Hot" : "Cold"); return false; } };

        if (upside)
            reverse = reverse.Reverse().ToArray();
        foreach (Func<bool> func in reverse)
            func();
        //Necessary for ChooseTexture case
        Module.OnActivate += delegate {
            canPress = true;
        };
        UpdateChildren();
    }

    private KMSelectable.OnInteractHandler ButtonHandler(KMSelectable selectable, int method, int r = 0)
    {
        return delegate
        {
            switch (method)
            {
                case 0:
                    if (wait) return false;
                    if (c == 0) queue.Enqueue(ButtonPress(selectable));
                    break;
                case 1:
                    var selectables = new[] { Cold, Hot, Faucet, Pipe };
                    if (c == 0) ChooseTexture(r, Array.IndexOf(selectables, selectable));
                    break;
            }
            return false;
        };
    }
    #endregion

    #region Coroutines
    private IEnumerator CheckForTurn()
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

    protected IEnumerable ButtonPress(KMSelectable knob)
    {
        knob.AddInteractionPunch(0.5f);
        Audio.PlaySoundAtTransform("valve_spin", knob.transform);

        if (!SOLVED)
        {
            canPress = false;
            if ((knob == Hot && knob2Turn[curKnob]) || (knob == Cold && !knob2Turn[curKnob]))
            {
                coroutine = KnobTurn(knob, 2.5f);
                while (coroutine.MoveNext())
                    yield return coroutine.Current;
                hotP += knob == Hot ? 2.5f : 0;
                coldP += knob == Cold ? 2.5f : 0;
                if (curKnob == 2)
                {
                    if (rotate && c == 0 && spin > 1500)
                    {
                        c = 1;
                        WaitForSol();
                        yield break;
                    }
                    Solve();
                }
                else if (rotate && c == 0 && spin > 1295)
                {
                    c = 2;
                    WaitForSol();
                    yield break;
                }
                else if (rotate && c == 0)
                {
                    c = 3;
                    WaitForSol();
                    yield break;
                }
                curKnob++;
            }
            else
            {
                coroutine = KnobTurn(knob, -2.5f);
                while (coroutine.MoveNext())
                    yield return coroutine.Current;
                hotP += knob == Hot ? -2.5f : 0;
                coldP += knob == Cold ? -2.5f : 0;
                coroutine = KnobTurn(Cold, -coldP);
                coroutine2 = KnobTurn(Hot, -hotP);
                while (coroutine.MoveNext() && coroutine2.MoveNext())
                {
                    yield return new [] { coroutine.Current, coroutine2.Current};
                }
                hotP = 0;
                coldP = 0;
                Module.HandleStrike();
                processingInput = false;
                curKnob = 0;
            }
            canPress = true;
        }
        yield return null;
    }

    private IEnumerator KnobTurn(KMSelectable r, float turn)
    {
        var hold = 0;
        
        while (hold != 15)
        {
            yield return new WaitForSeconds(0.001f);
            r.transform.Rotate(Vector3.up, turn);
            hold += 1;
        }
        
        yield return null;
    }

    private IEnumerator Spinning(KMSelectable r)
    {
        var t = 2.5f;
        if (c == 3) t = -t;
        while (isActiveAndEnabled)
        {
            rotating = true;
            r.transform.Rotate(Vector3.up, t);
            yield return null;
        }
    }

    private IEnumerator Spin()
    {
        yield return new WaitUntil(() => rotate);
        while (rotate)
        {
            spin = UnityEngine.Random.Range(0, 2000);
            forceSpin = spin;
            yield return new WaitForSeconds(seconds);
        }
    }

    private IEnumerator Timer()
    {
        dT = 0;
        /*Due to the Timer being stopped after rotation,
        * canPress == false is likely not needed here.
        * But since everything's working, I'll leave it be for now*/
        while (h == true && canPress == false)
        {
            dT++;
            yield return new WaitForSeconds(1);
            if (dT > 10) yield break;
        }
    }
    #endregion

    #region Faulty Functions

    private void WaitForSol()
    {
        canPress = false;
        var r = UnityEngine.Random.Range(0, 2);
        var k = new[] { Hot, Cold };
        coroutine = Spinning(k[r]);
        StartCoroutine(coroutine);
        if (c == 1)
        {
            DebugLog("Debug: Rotation error, reverse the inputs to disable.");
            curKnob = 0;
            Hot.OnInteract = delegate () { Required(true); return false; };
            Cold.OnInteract = delegate () { Required(false); return false; };
        }
        if (c == 2)
        {
            if (!processingInput) DebugLog("Button not responding, please hold the button still to reorientate the button.");
            else Debug.LogFormat("<Faulty Sink #{0}> Rotation Rule 2 applied, but ignored by Twitch Plays.", moduleID);
            wait = true;
            HotHandler = Hot.OnInteract;
            ColdHandler = Cold.OnInteract;
            coroutine2 = Timer();
            k[r].OnInteract = delegate () { h = true; StartCoroutine(coroutine2); return false; };
            k[r].OnInteractEnded = WaitForSelect();
        }
        if (c == 3)
        {
            DebugLog("Invalid button selected, please select a valid button.");
            HotHandler = Hot.OnInteract;
            ColdHandler = Cold.OnInteract;
            k[r].OnInteract = delegate () { Module.HandleStrike(); processingInput = false; return false; };
            k[(r + 1) % 2].OnInteract = delegate () { Fix(k[r]); return false; };
        }
    }

    private void Fix(KMSelectable selectable)
    {
        if (c == 3)
        {
            StopCoroutine(coroutine);
            rotating = false;
            canPress = true;
            c = 0;
            curKnob++;
            Hot.OnInteract = HotHandler;
            Cold.OnInteract = ColdHandler;
        }
    }

    private void Required(bool r)
    {
        var apply = new Func<bool>[] { () => { if (knob2Turn[2] == r) curKnob++; return false; },
        () => { if (knob2Turn[1] == r) curKnob++; else curKnob--; return false; },
        () => { if (knob2Turn[0] == r) Solve(); else curKnob = 0; return false; }
        };
        DebugLog("Debug: Pressing {0}", r ? "Hot" : "Cold");
        apply[curKnob]();
        if (curKnob == 0)
            DebugLog("Incorrect sequence, resetting.");
    }

    private Action WaitForSelect()
    {
        if (c == 1) return null;
        if (curKnob == 2) return null;
        return delegate
        {
            if (dT >= 3 && dT <= 5)
            {
                Skip();
            }
            else if (c == 2 && canPress == false)
            {
                StopCoroutine(coroutine2);
                DebugLog("Reset failed.");
                if (spin > 1500 && dT > 5) Module.HandleStrike();
            }
            else StopCoroutine(coroutine2);
            h = false;
        };
    }

    private void Skip()
    {
        canPress = true;
        wait = false;
        StopCoroutine(coroutine);
        StopCoroutine(coroutine2);
        rotating = false;
        Hot.OnInteract = HotHandler;
        Cold.OnInteract = ColdHandler;
        Hot.OnInteractEnded = null;
        Cold.OnInteractEnded = null;
        curKnob++;
        c = 0;
        if (!processingInput) DebugLog("Button reorientated.");
        else Debug.LogFormat("<Faulty Sink #{0}> Button reorientated.", moduleID);
    }

    private void FaultyPVC(int num)
    {
        var hot = new Func<bool>[] { () => { Hot.transform.Rotate(Vector3.up, -50f); curKnob = 1; return false; },
        () => { Module.HandleStrike(); DebugLog("Warning: Invalid button [HotMesh] selected."); processingInput = false;  return false; }};
        var cold = new Func<bool>[] { () => { Module.HandleStrike(); processingInput = false; return false; } };
        var basin = new Func<bool>[] { () => { Module.HandleStrike(); DebugLog("Warning: Invalid button [BasinMesh] selected"); processingInput = false; return false; },
        () => { Solve(); return false; } };
        var ind = new[] { hot, basin, cold.Concat(cold).ToArray() };
        /* I'm not entirely sure why canPress is used here
         * The main method is never accessed, so canPress would always be true */
        var condition = new bool[][] { new[] { true, true, canPress }, new[] { canPress, true, canPress } };
        if (condition[curKnob][num])
        {
            ind[num][curKnob]();
        }
    }

    private void ChooseTexture(int r, int s)
    {
        if (o) return;
        var ren = new[] { ColdMesh, HotMesh, FaucetMesh, PipeMesh };

        if (r != s && !knob2Turn[0] && s < 2)
        {
            Module.HandleStrike();
            processingInput = false;
            DebugLog("NullReferenceException: Object reference not set to an instance of an object.");
        }
        else if (r == s && !knob2Turn[0])
        {
            Module.HandleStrike();
            processingInput = false;
            DebugLog("Warning: Invalid button [{0}] selected.", ren[(r + 1) % 2].material.mainTexture);
            if (!knob2Turn[1]) ren[s].material = null;
        }
        else if (!knob2Turn[0])
        {
            hold = ren[s].material.mainTexture;
            buttonMasher = ren[s].material;
            processingInput = false;
            DebugLog("Texture [{0}] chosen.", hold.name);
            knob2Turn[0] = true;
        }
        else if (r == s)
        {
            ren[s].material.mainTexture = hold;
            knob2Turn[0] = false;
            knob2Turn[1] = true;
            DebugLog("Texture [{0}] applied.", hold.name);
        }
        else
        {
            knob2Turn[0] = false;
            DebugLog("Warning: Texture {0} applied.", s < 2 ? "[" + hold.name + "] cannot be" : "already");
        }

        if (knob2Turn[1])
        {
            if (ColdMesh.material.mainTexture == HotMesh.material.mainTexture)
            {
                ren[s].material = buttonMasher;
                o = true;
                Module.GetComponent<KMSelectable>().Children[1] = null;
                Module.GetComponent<KMSelectable>().Children[7] = null;
                UpdateChildren();
                Steps(false);
            }
            else knob2Turn[2] = false;
        }
    }

    public void UpdateChildren()
    {
        GetComponent<KMSelectable>().UpdateChildren();
#if UNITY_EDITOR
        for (int i = 0; i < GetComponent<KMSelectable>().Children.Count(); i++)
        {
            var selectable = GetComponent<KMSelectable>().Children[i];
            if (selectable == null && GetComponent<TestSelectable>().Children[i] != null)
            {
                Destroy(GetComponent<TestSelectable>().Children[i].GetComponentInChildren<TestSelectableArea>());
                GetComponent<TestSelectable>().Children[i] = null;
            }
            else if (selectable != null && GetComponent<TestSelectable>().Children[i] == null)
            {
                selectable.gameObject.AddComponent<TestSelectable>();
                GetComponent<TestSelectable>().Children[i] = selectable.GetComponent<TestSelectable>();
            }
        }
#endif
    }

    #endregion

    private void Solve()
    {
        StopAllCoroutines();
        SOLVED = true;
        rotate = false;
        rotating = false;
        Module.HandlePass();
        DebugLog("The module has been disarmed.");
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[{0}Sink #{1}] {2}", isFaulty ? "Faulty " : "", moduleID, logData);
    }
}
