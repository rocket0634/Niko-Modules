using KMBombInfoExtensions;
using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Sink : MonoBehaviour
{
    public enum Type
    {
        Normal,
        Faulty
    }
    private SinkSettings Settings = new SinkSettings(); 
    private static int Sink_moduleIdCounter = 1;
    private static int FSink_moduleIdCounter = 1;
    private int Sink_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Cold, Hot;
    private KMSelectable Basin, Pipe, Faucet;
    public MeshRenderer ColdMesh, HotMesh, FaucetMesh, PipeMesh;
    public TextMesh C, H;
    public Texture[] knobColors = new Texture[3];
    public Type moduleType;
    private string[] textureList = { "Copper", "Stainless Steel", "Gold-Plated" };
    private string[] colorList = { "Copper", "Iron", "PVC" };
    // false = cold, hot = true
    protected bool knob1;
    protected bool knob2;
    protected bool knob3;
    protected int curknob;
    protected bool[] Rules;
    protected bool knob2turn;
    protected bool canPress;
    protected bool SOLVED = false;
    protected bool o;
    private bool rotate, rotating, wait, h;
    private int ColKnobs, ColFaucet, ColPipe, spin, c;
    private int[] selectedRules;
    private Texture hold;
    private IEnumerator coroutine;
    protected float coldP, hotP, dT;
    private Queue<IEnumerable> queue = new Queue<IEnumerable>();

    private string TwitchHelpMessage = "Use \"!{0} Hot\" or \"!{0} H\" to turn the Hot knob, \"!{0} Cold\" or \"!{0} C\" to turn the Cold knob, \"!{0} Hot Cold Hot\" or \"!{0} H C H\" to turn the knobs in the sequence hot cold hot";

    public IEnumerator ProcessTwitchCommand(string TPInput)
    {
        string[] taps = TPInput.ToLowerInvariant().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        List<KMSelectable> tapList = new List<KMSelectable>();

        foreach (string tap in taps)
        {
            yield return null;
            if (tap == "hot" || tap == "h")
            {
                yield return new KMSelectable[] { Hot };
            }
            else if (tap == "cold" || tap == "c")
            {
                yield return new KMSelectable[] { Cold };
            }
            //TP is in no way compatible with Faulty Sink atm...
            else if (moduleType == Type.Faulty)
            {
                switch (tap)
                {
                    case "faucet":
                    case "f":
                        if (Faucet != null) yield return new KMSelectable[] { Faucet };
                        break;
                    case "pipe":
                    case "p":
                    case "drain":
                    case "d":
                    case "dp":
                        if (Pipe != null) yield return new KMSelectable[] { Pipe };
                        break;
                    case "basin":
                    case "sink":
                    case "s":
                    case "b":
                        if (Basin != null) yield return new KMSelectable[] { Basin };
                        break;
                }
            }
            else
            {
                yield break;
            }
            yield return new WaitUntil(() => canPress);
        }
    }

    protected void Start()
    {
        ModConfig modConfig = new ModConfig("SinkSettings", typeof(SinkSettings));
        Settings = (SinkSettings)modConfig.Settings;
        if (Settings.fault == "1")
        {
            moduleType = UnityEngine.Random.Range(0,2).Equals(0)? Type.Normal : Type.Faulty;
            BombModule.ModuleDisplayName = "Faulty Sink";
            BombModule.ModuleType = "Faulty Sink";
        }

        ColKnobs = UnityEngine.Random.Range(0, 3);
        ColFaucet = UnityEngine.Random.Range(0, 3);
        ColPipe = UnityEngine.Random.Range(0, 3);

        Rules = new bool[6] { BombInfo.GetOffIndicators().Contains("NSA"), BombInfo.GetSerialNumberLetters().Any("AEIOU".Contains), (ColKnobs == 2), (ColFaucet == 1), (ColPipe == 0), (BombInfo.GetPorts().Contains("HDMI") || BombInfo.GetPorts().Contains("RJ45")) };

        if (moduleType == Type.Normal && BombModule.GetComponent<KMSelectable>().Children.Count() > 2)
        {
            for (int i = 1; i < 8; i++)
            {
                if (i != 2) BombModule.GetComponent<KMSelectable>().Children[i] = null;
            }
            Sink_moduleId = Sink_moduleIdCounter++;
        }
        if (moduleType == Type.Faulty)
        {
            Sink_moduleId = FSink_moduleIdCounter++;
            spin = UnityEngine.Random.Range(0, 20);
            if (spin > 15) rotate = true;
            var faulty = UnityEngine.Random.Range(0, 5);
            Faucet = BombModule.GetComponent<KMSelectable>().Children[1];
            Basin = BombModule.GetComponent<KMSelectable>().Children[3];
            Pipe = BombModule.GetComponent<KMSelectable>().Children[7];
            BombModule.GetComponent<KMSelectable>().Children[1] = null;
            for (int i = 3; i < 8; i++) BombModule.GetComponent<KMSelectable>().Children[i] = null;
            switch (faulty)
            {
                case 0:
                    PipeMesh.material.color = Color.black;
                    ColdMesh.material.mainTexture = knobColors[ColKnobs];
                    HotMesh.material.mainTexture = knobColors[ColKnobs];
                    FaucetMesh.material.mainTexture = knobColors[ColFaucet];
                    for (int i = 3; i < 6; i++) BombModule.GetComponent<KMSelectable>().Children[i] = Basin;
                    Basin.OnInteract += delegate () { FaultyPVC(1); return false; };
                    Hot.OnInteract += delegate () { FaultyPVC(0); return false; };
                    Cold.OnInteract += delegate () { FaultyPVC(2); return false; };
                    BombModule.GetComponent<KMSelectable>().UpdateChildren();
                    goto start;
                case 1:
                    ColPipe = 2;
                    PipeMesh.material.color = new Color(0, 157/255f, 1);
                    Rules[4] = false;
                    Rules = Rules.Select(x => !x).ToArray();
                    break;
                case 2:
                    var r = UnityEngine.Random.Range(0, 2);
                    ColdMesh.material.mainTexture = knobColors[ColKnobs];
                    HotMesh.material.mainTexture = knobColors[ColKnobs];
                    if (r == 0) ColdMesh.material = null;
                    else HotMesh.material = null;
                    BombModule.GetComponent<KMSelectable>().Children[1] = Faucet;
                    BombModule.GetComponent<KMSelectable>().Children[7] = Pipe;
                    BombModule.GetComponent<KMSelectable>().UpdateChildren();
                    while (ColFaucet == ColPipe && ColKnobs == ColPipe)
                    {
                        ColFaucet = UnityEngine.Random.Range(0, 3);
                        ColPipe = UnityEngine.Random.Range(0, 3);
                    }
                    if (!ColFaucet.Equals(ColKnobs) && !ColPipe.Equals(ColKnobs))
                    {
                        ColPipe = ColKnobs;
                    }
                    FaucetMesh.material.mainTexture = knobColors[ColFaucet];
                    PipeMesh.material.mainTexture = knobColors[ColPipe];
                    if (!ColPipe.Equals(ColKnobs)) PipeMesh.material.mainTexture = null;
                    Faucet.OnInteract = ButtonHandler(Faucet, 1, r);
                    Pipe.OnInteract = ButtonHandler(Pipe, 1, r);
                    Cold.OnInteract = ButtonHandler(Cold, 1, r);
                    Hot.OnInteract = ButtonHandler(Hot, 1, r);
                    Rules[3] = ColFaucet.Equals(1);
                    Rules[4] = ColPipe.Equals(0);
                    goto start;
                case 3:
                    rotate = false;
                    var s = UnityEngine.Random.Range(0, 2);
                    var selectable = new[] { Faucet, Pipe };
                    Hot.OnInteract += delegate () { BombModule.HandleStrike(); DebugLog("Warning: Overheating"); return false; };
                    if (s == 0)
                    {
                        Hot = Faucet;
                        BombModule.GetComponent<KMSelectable>().Children[1] = Faucet;
                    }
                    else
                    {
                        Hot = Pipe;
                        BombModule.GetComponent<KMSelectable>().Children[7] = Pipe;
                    }
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
                    Steps();
                    goto start;
                case 4:
                    BombModule.transform.Rotate(0, 180, 0);
                    Rules = Rules.Reverse().ToArray();
                    switch (BombInfo.GetBatteryCount())
                    {
                        case 0:
                        case 1:
                            selectedRules = new[] { 4, 6, 5 };
                            break;
                        case 2:
                        case 3:
                            selectedRules = new[] { 1, 3, 5 };
                            Rules = Rules.Select(x => !x).ToArray();
                            break;
                        case 4:
                        case 5:
                            selectedRules = new[] { 2, 6, 3 };
                            Rules = Rules.Select(x => !x).ToArray();
                            break;
                        default:
                            selectedRules = new[] { 4, 1, 2 };
                            break;
                    }
                    ColdMesh.material.mainTexture = knobColors[ColKnobs];
                    HotMesh.material.mainTexture = knobColors[ColKnobs];
                    FaucetMesh.material.mainTexture = knobColors[ColFaucet];

                    knob1 = Rules[selectedRules[0] - 1];
                    knob2 = Rules[selectedRules[1] - 1];
                    knob3 = Rules[selectedRules[2] - 1];

                    if (ColPipe != 2) PipeMesh.material.mainTexture = knobColors[ColPipe];
                    DebugLog("Third Knob: {0}", knob1 ? "Hot" : "Cold");
                    DebugLog("Second Knob: {0}",  knob2 ? "Hot" : "Cold");
                    DebugLog("First Knob: {0}", knob3 ? "Hot" : "Cold");
                    DebugLog("Knobs are {0}, Faucet is {1}, Drain Pipe is {2}", textureList[ColKnobs], textureList[ColFaucet], colorList[ColPipe]);
                    StartCoroutine(CheckForTurn());
                    Cold.OnInteract = ButtonHandler(Cold, 0);
                    Hot.OnInteract = ButtonHandler(Hot, 0);
                    canPress = true;
                    BombModule.GetComponent<KMSelectable>().UpdateChildren();
                    goto start;
            }
        }

        ColdMesh.material.mainTexture = knobColors[ColKnobs];
        HotMesh.material.mainTexture = knobColors[ColKnobs];
        FaucetMesh.material.mainTexture = knobColors[ColFaucet];

        if (ColPipe != 2) PipeMesh.material.mainTexture = knobColors[ColPipe];

        Steps();
        
        start: return;
    }

    private void Steps()
    {
        StartCoroutine(CheckForTurn());
        Cold.OnInteract = ButtonHandler(Cold, 0);
        Hot.OnInteract = ButtonHandler(Hot, 0);
        //check what the serial ends with and make an integer for it
        int Batteries = BombInfo.GetBatteryCount();//BombInfo.GetSerialNumberNumbers().Last();

        switch (Batteries)
        {
            case 0:
            case 1:
                knob1 = Rules[1];
                knob2 = Rules[0];
                knob3 = Rules[3];
                break;
            case 2:
            case 3:
                knob1 = !Rules[2];
                knob2 = !Rules[5];
                knob3 = !Rules[1];
                break;
            case 4:
            case 5:
                knob1 = !Rules[4];
                knob2 = !Rules[2];
                knob3 = !Rules[0];
                break;
            default:
                knob1 = Rules[4];
                knob2 = Rules[5];
                knob3 = Rules[3];
                break;
        }

        DebugLog("Knobs are {0}, Faucet is {1}, Drain Pipe is {2}", textureList[ColKnobs], textureList[ColFaucet], colorList[ColPipe]);
        DebugLog("First Knob: {0}", knob1 ? "Hot" : "Cold");
        DebugLog("Second Knob: {0}", knob2 ? "Hot" : "Cold");
        DebugLog("Third Knob: {0}", knob3 ? "Hot" : "Cold");
        canPress = true;
        BombModule.GetComponent<KMSelectable>().UpdateChildren();
    }

    private KMSelectable.OnInteractHandler ButtonHandler(KMSelectable selectable, int method, int r = 0)
    {
        //StopCoroutine(Timer());
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
                /*case 2:
                    if (c == 1 || c == 2)
                    {
                        h = true;
                        StartCoroutine(Timer());
                        StopCoroutine(Timer());
                    }
                    break;*/
            }
            return false;
        };
    }

    /*private Action WaitForSelect()
    {
        if (c == 1) return null;
        if (curknob == 2) return null;
        return delegate
        {
            if (dT >= 3 && dT <= 5)
            {
                h = false;
                canPress = true;
                wait = false;
                StopCoroutine(Timer());
                StopCoroutine(coroutine);
                Hot.OnInteract = ButtonHandler(Hot, 0);
                Cold.OnInteract = ButtonHandler(Cold, 0);
                c = 0;
            }
            else if (c == 2 && canPress == false)
            {
                StopCoroutine(Timer());
                DebugLog("Reset failed.");
                if (spin > 1500 && dT > 5) BombModule.HandleStrike();
            }
            else StopCoroutine(Timer());
        };
    }

    private IEnumerator Timer()
    {
        dT = 0;
        while (h == true && canPress == false)
        {
            dT++;
            DebugLog(dT.ToString());
            yield return new WaitForSeconds(1);
            if (dT > 10) yield break;
        }
    }*/

    protected IEnumerable ButtonPress(KMSelectable knob)
    {
        knob.AddInteractionPunch(0.5f);
        KMAudio.PlaySoundAtTransform("valve_spin", knob.transform);

        if (!SOLVED)
        {
            canPress = false;
            StartCoroutine(KnobTurn(knob));
        }
        yield return null;
    }

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

    private void WaitForSol()
    {
        canPress = false;
        var r = UnityEngine.Random.Range(0, 2);
        var k = new[] { Hot, Cold };
        coroutine = Spinning(k[r]);
        StartCoroutine(coroutine);
        if (c == 1)
        {
            curknob = 0;
            Hot.OnInteract += delegate () { required(true, k[r]); return false; };
            Cold.OnInteract += delegate () { required(false, k[r]); return false; };
        }
        /*if (c == 2)
        {
            wait = true;
            k[r].OnInteract = ButtonHandler(k[r], 2);
            k[r].OnInteractEnded = WaitForSelect();
        }
        if (c == 3)
        {
            k[r].OnInteract += delegate () { BombModule.HandleStrike(); return false; };
            k[(r + 1) % 2].OnInteract += delegate () { Fix(k[r]); return false; };
        }*/
    }

    /*private void Fix(KMSelectable selectable)
    {
        if (c == 3)
        {
            StopCoroutine(coroutine);
            canPress = true;
            c = 0;
            selectable.OnInteract += delegate () { ButtonHandler(selectable, 0); return false; };
        }
    }*/

    private bool required(bool r, KMSelectable k)
    {
        /*StopCoroutine(coroutine);
        DebugLog("test");*/
        switch (curknob)
        {
            case 0:
                if (knob3 == r) curknob++;
                break;
            case 1:
                if (knob2 == r) curknob++;
                else curknob--;
                break;
            case 2:
                if (knob1 == r)
                {
                    StopAllCoroutines();
                    BombModule.HandlePass();
                    SOLVED = true;
                    rotate = false;
                    rotating = false;
                    DebugLog("The module has been defused.");
                }
                else curknob = 0;
                break;
        }
        return false;
    }

    private IEnumerator KnobTurn(KMSelectable r)
    {
        yield return null;
        var hold = 0;
        if ((r == Hot && knob2turn) || r == Cold && !knob2turn)
        {
            if (r == Hot) hotP += 2.5f;
            else coldP += 2.5f;
            while (hold != 15)
            {
                yield return new WaitForSeconds(0.001f);
                r.transform.Rotate(Vector3.up, 2.5f);
                hold += 1;
            }
            if (curknob == 2)
            {
                if (rotate && c == 0 && spin > 1500)
                {
                    c = 1;
                    WaitForSol();
                    yield break;
                }
                BombModule.HandlePass();
                SOLVED = true;
                DebugLog("The module has been defused.");
            }
            else
            {
                /*DebugLog("test2" + " " + curknob.ToString());
                if (rotate && c == 0 && curknob < 2)
                {
                    c = 2;
                    WaitForSol();
                    yield break;
                }
                if (rotate && c == 0)
                {
                    c = 3;
                    WaitForSol();
                    yield break;
                }*/
                curknob++;
            }
        }
        else
        {
            while (hold != 15)
            {
                yield return new WaitForSeconds(0.001f);
                r.transform.Rotate(Vector3.up, -2.5f);
                hold += 1;
            }
            if (r == Hot) hotP -= 2.5f;
            else coldP -= 2.5f;
            while (hold != 30)
            {
                yield return new WaitForSeconds(0.001f);
                Hot.transform.Rotate(Vector3.up, -hotP);
                Cold.transform.Rotate(Vector3.up, -coldP);
                hold += 1;
            }
            hotP = 0;
            coldP = 0;
            BombModule.HandleStrike();
            curknob = 0;
        }
        hold = 0;
        canPress = true;
    }

    private void Update()
    {
        if (curknob == 0)
        {
            knob2turn = knob1;
        }
        else if (curknob == 1)
        {
            knob2turn = knob2;
        }
        else if (curknob == 2)
        {
            knob2turn = knob3;
        }
        if (rotate) spin = UnityEngine.Random.Range(0, 2000);
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        if (moduleType == Type.Normal) Debug.LogFormat("[Sink #{0}]: {1}", Sink_moduleId, logData);
        else Debug.LogFormat("[Faulty Sink #{0}]: {1}", Sink_moduleId, logData);
    }

    private void FaultyPVC(int num)
    {
        switch (num)
        {
            case 0:
                if (curknob == 1 && canPress)
                {
                    BombModule.HandleStrike();
                    DebugLog("Warning: Invalid button [HotMesh] selected.");
                }
                else if (curknob == 0)
                {
                    Hot.transform.Rotate(Vector3.up, -50f);
                    curknob = 1;
                }
                break;
            case 1:
                if (curknob == 0)
                {
                    BombModule.HandleStrike();
                    DebugLog("Warning: Invalid button [basinMesh] selected");
                }
                else if (curknob == 1)
                {
                    BombModule.HandlePass();
                    DebugLog("The Module has been defused.");
                    canPress = false;
                }
                break;
            default:
                if (canPress) BombModule.HandleStrike();
                break;
        }
    }

    private void ChooseTexture(int r, int s)
    {
        if (o) return;
        var ren = new[] { ColdMesh, HotMesh, FaucetMesh, PipeMesh };
        Texture c = new Texture();
        if (r != s) c = ren[s].material.mainTexture;
        var d = ren[(r + 1) % 2].material.mainTexture;
        switch (s)
        {
            case 0:
                if (!knob1 && r != 0)
                {
                    BombModule.HandleStrike();
                    DebugLog("NullReferenceException: Object reference not set to an instance of an object.");
                }
                else if (!knob1)
                {
                    BombModule.HandleStrike();
                    DebugLog("Warning: Invalid button [ColdMesh] selected.");
                }
                else if (r == 0)
                {
                    ColdMesh.material.mainTexture = hold;
                    knob1 = false;
                    knob2 = true;
                    DebugLog("Texture [{0}] applied.", hold.name);
                }
                else
                {
                    knob1 = false;
                    DebugLog("Warning: Texture [{0}] cannot be applied", hold.name);
                }
                break;
            case 1:
                if (!knob1 && r == 0)
                {
                    BombModule.HandleStrike();
                    DebugLog("NullReferenceException: Object reference not set to an instance of an object.");
                }
                else if (!knob1)
                {
                    BombModule.HandleStrike();
                    DebugLog("Warning: Invalid button [HotMesh] selected.");
                }
                else if (r == 1)
                {
                    HotMesh.material.mainTexture = hold;
                    knob1 = false;
                    knob2 = true;
                    DebugLog("Texture [{0}] applied.", hold.name);
                }
                else
                {
                    knob1 = false;
                    DebugLog("Warning: Texture [{1}] cannot be applied", hold.name);
                }
                break;
            case 2:
                if (!knob1)
                {
                    hold = FaucetMesh.material.mainTexture;
                    DebugLog("Texture [FaucetMesh] chosen.");
                    knob1 = true;
                }
                else
                {
                    knob1 = false;
                    DebugLog("Warning: Texture already applied.");
                }
                break;
            case 3:
                if (!knob1)
                {
                    hold = PipeMesh.material.mainTexture;
                    DebugLog("Texture [PipeMesh] chosen.");
                    knob1 = true;
                }
                else
                {
                    knob1 = false;
                    DebugLog("Warning: Texture already applied.");
                }
                break;
        }
        if (knob2)
        {
            if (ColdMesh.material.mainTexture == HotMesh.material.mainTexture)
            {
                o = true;
                BombModule.GetComponent<KMSelectable>().Children[1] = null;
                BombModule.GetComponent<KMSelectable>().Children[7] = null;
                BombModule.GetComponent<KMSelectable>().UpdateChildren();
                Steps();
            }
            else knob2 = false;
        }
    }
}

class SinkSettings
{
    public string fault = "0";
}

class ModConfig
{
    public ModConfig(string name, Type settingsType)
    {
        _filename = name;
        _settingsType = settingsType;
    }

    readonly string _filename = null;
    readonly Type _settingsType = null;

    string SettingsPath
    {
        get
        {
            return Path.Combine(Path.Combine(Application.persistentDataPath, "Modsettings"), _filename + ".json");
        }
    }

    public object Settings
    {
        get
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Activator.CreateInstance(_settingsType), Formatting.Indented));
                }

                return JsonConvert.DeserializeObject(File.ReadAllText(SettingsPath), _settingsType);
            }
            catch
            {
                return Activator.CreateInstance(_settingsType);
            }
        }

        set
        {
            if (value.GetType() == _settingsType)
            {
                File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(value, Formatting.Indented));
            }
        }
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(Settings, Formatting.Indented);
    }
}