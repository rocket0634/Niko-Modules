using System.Collections.Generic;
using System.Linq;
using UnityEngine;
partial class FaultySink : SinkBase
{
	private static int moduleIDCounter = 1;
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Use !{0} sinkcommands to see Sink's help message. You may use !{0} faucet pipe sink or !{0} fps to interact with the module. Determine rather to use the pipe or faucet using !{0} highlight. If a knob is rotating, any further inputs will be dropped.";
#pragma warning restore 414
    private KMSelectable Faucet { get { return _scaffold.Faucet; } }
    private KMSelectable Pipe { get { return _scaffold.Pipe; } }
    private MeshRenderer[] ren { get { return new[] { ColdMesh, HotMesh, FaucetMesh, PipeMesh }; } }
    [SerializeField]
    private int faulty;
    protected override void SetFields()
    {
        _moduleId = moduleIDCounter++;
        _selectableChildren = new[] { Cold, null, Hot, null, null, null, null, null, null };
    }

    protected override void Steps()
    {
        Hijack();
    }

    private void Hijack()
    {
        RunSpin();
        faulty = Random.Range(0, 5);
        System.Action[] faultyCases = new System.Action[]
        {
            RunUnknown,
            () =>
            {
                ColPipe = 2;
                PipeMesh.material.color = new Color(0, 157/255f, 1);
                Rules[4] = false;
                Rules = Rules.Select(x => !x).ToArray();
                Log("Debug: PVC reported as blue.");
                base.Steps();
                base.ApplyTextures(Enumerable.Range(0, 4));
            },
            RunSetupTexture,
            RunOverheat,
            RunReverse
        };
        faultyCases[faulty]();
    }

    // Prevent ApplyTextures from being called by the base class
    protected override void ApplyTextures(IEnumerable<int> nums){}

    private void UpdateHandlers(bool add, System.Func<KMSelectable, bool> func = null, params KMSelectable[] selectables)
    {
        foreach (KMSelectable selectable in selectables)
            knobs.Add(selectable, new Temp(selectable));
        if (add)
        {
            Handlers.Clear();
            foreach (KMSelectable key in knobs.Keys)
            {
                Handlers.Add(key, () => func(key));
                key.OnInteract += Handlers[key];
            }
        }
        else
            foreach (KMSelectable key in knobs.Keys)
                key.OnInteract -= Handlers[key];

    }
}