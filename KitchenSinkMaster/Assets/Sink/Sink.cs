using KMBombInfoExtensions;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Sink : MonoBehaviour
{
    private static int Sink_moduleIdCounter = 1;
    private int Sink_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Cold;
    public KMSelectable Hot;
    public MeshRenderer ColdMesh;
    public MeshRenderer HotMesh;
    public MeshRenderer FaucetMesh;
    public MeshRenderer PipeMesh;
    // false = cold, hot = true
    protected bool knob1;
    protected bool knob2;
    protected bool knob3;
    protected int curknob;
    protected bool[] Rules;
    protected bool knob2turn;
    protected Color Copper = new Color(0.5f, 0.25f, 0);
    protected Color Gold = new Color(0.5f, 0.5f, 0);
    protected bool SOLVED = false;

    private string TwitchHelpMessage = "Use \"!{0} Hot\" or \"!{0} H\" to turn the Hot knob, \"!{0} Cold\" or \"!{0} C\" to turn the Cold knob, \"!{0} Hot Cold Hot\" or \"!{0} H C H\" to turn the knobs in the sequence hot cold hot";

    public KMSelectable[] ProcessTwitchCommand(string TPInput)
    {
        string[] taps = TPInput.ToLowerInvariant().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        List<KMSelectable> tapList = new List<KMSelectable>();

        foreach (string tap in taps)
        {
            if (tap == "hot" || tap == "h")
            {
                tapList.Add(Hot);
            }
            else if (tap == "cold" || tap == "c")
            {
                tapList.Add(Cold);
            }
            else
            {
                return null;
            }
        }

        return tapList.Any() ? tapList.ToArray() : null;
    }

    protected void Start()
    {
        Sink_moduleId = Sink_moduleIdCounter++;

        int ColKnobs = UnityEngine.Random.Range(1, 4);
        int ColFaucet = UnityEngine.Random.Range(1, 4);
        int ColPipe = UnityEngine.Random.Range(1, 4);
        string ColKnobsName = "";
        string ColFaucetName = "";
        string ColPipeName = "";

        if (ColKnobs == 1)
        {
            ColdMesh.material.color = Copper;
            HotMesh.material.color = Copper;
            ColKnobsName = "Copper";
        }
        if (ColKnobs == 2)
        {
            ColdMesh.material.color = Color.grey;
            HotMesh.material.color = Color.grey;
            ColKnobsName = "Stainless Steel";
        }
        if (ColKnobs == 3)
        {
            ColdMesh.material.color = Gold;
            HotMesh.material.color = Gold;
            ColKnobsName = "Gold-Plated";
        }

        if (ColFaucet == 1)
        {
            FaucetMesh.material.color = Copper;
            ColFaucetName = "Copper";
        }
        if (ColFaucet == 2)
        {
            FaucetMesh.material.color = Color.grey;
            ColFaucetName = "Stainless Steel";
        }
        if (ColFaucet == 3)
        {
            FaucetMesh.material.color = Gold;
            ColFaucetName = "Gold-Plated";
        }

        if (ColPipe == 1)
        {
            PipeMesh.material.color = Copper;
            ColPipeName = "Copper";
        }
        if (ColPipe == 2)
        {
            PipeMesh.material.color = Color.grey;
            ColPipeName = "Iron";
        }
        if (ColPipe == 3)
        {
            PipeMesh.material.color = Color.black;
            ColPipeName = "PVC";
        }


        Rules = new bool[6] { BombInfo.GetOffIndicators().Contains("NSA"), BombInfo.GetSerialNumberLetters().Any("AEIOU".Contains), (ColKnobs == 3), (ColFaucet == 2), (ColPipe == 1), (BombInfo.GetPorts().Contains("HDMI") || BombInfo.GetPorts().Contains("RJ45")) };

        Cold.OnInteract += HandlePressCold;
        Hot.OnInteract += HandlePressHot;
        //check what the serial ends with and make an integer for it
        int Batteries = BombInfo.GetBatteryCount();//BombInfo.GetSerialNumberNumbers().Last();

        if (Batteries == 0 || Batteries == 1)
        {
            knob1 = Rules[1];
            knob2 = Rules[0];
            knob3 = Rules[3];
        }
        else if (Batteries == 2 || Batteries == 3)
        {
            knob1 = Rules[2];
            knob2 = Rules[5];
            knob3 = Rules[1];
            knob1 = !knob1;
            knob2 = !knob2;
            knob3 = !knob3;
        }
        else if (Batteries == 4 || Batteries == 5)
        {
            knob1 = Rules[4];
            knob2 = Rules[2];
            knob3 = Rules[0];
            knob1 = !knob1;
            knob2 = !knob2;
            knob3 = !knob3;
        }
        else
        {
            knob1 = Rules[4];
            knob2 = Rules[5];
            knob3 = Rules[3];
        }

        DebugLog("Knobs are {0}, Faucet is {1}, Drain Pipe is {2}", ColKnobsName, ColFaucetName, ColPipeName);
        if (knob1)
        {
            DebugLog("First Knob: Hot");
        }
        else
        {
            DebugLog("First Knob: Cold");
        }

        if (knob2)
        {
            DebugLog("Second Knob: Hot");
        }
        else
        {
            DebugLog("Second Knob: Cold");
        }

        if (knob3)
        {
            DebugLog("Third Knob: Hot");
        }
        else
        {
            DebugLog("Third Knob: Cold");
        }
    }

    protected bool HandlePressCold()
    {
        Cold.AddInteractionPunch(0.5f);
        KMAudio.PlaySoundAtTransform("valve_spin", Cold.transform);

        if (!SOLVED)
        {
            if (knob2turn == false)
            {

                if (curknob == 2)
                {
                    BombModule.HandlePass();
                    SOLVED = true;
                    DebugLog("The module has been defused.");
                }
                else
                {
                    curknob++;
                }
            }
            else
            {
                BombModule.HandleStrike();
                curknob = 0;
            }
        }
        return false;
    }

    protected bool HandlePressHot()
    {
        Hot.AddInteractionPunch(0.5f);
        KMAudio.PlaySoundAtTransform("valve_spin", Hot.transform);

        if (!SOLVED)
        {
            if (knob2turn == true)
            {

                if (curknob == 2)
                {
                    BombModule.HandlePass();
                    SOLVED = true;
                    DebugLog("The module has been defused.");
                }
                else
                {
                    curknob++;
                }
            }
            else
            {
                BombModule.HandleStrike();
                curknob = 0;
            }
        }
        return false;
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
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Sink #{0}]: {1}", Sink_moduleId, logData);
    }
}