using KMBombInfoExtensions;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class FaultyBG : MonoBehaviour
{
    //Obsolete. Replaced with Backgrounds.cs
    //Hopefully it won't hurt to just keep it here, though.
    protected int ColButton;
    protected int ColBacking;
    private static int FBackgrounds_moduleIdCounter = 1;
    private int FBackgrounds_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Submit;
    public KMSelectable ButtonA;
    public KMSelectable ButtonB;
    public MeshRenderer ButtonAMesh;
    public MeshRenderer ButtonBMesh;
    public MeshRenderer BackingMesh;
    public TextMesh CounterText;
    public TextMesh ButtonATextMesh;
    public TextMesh ButtonBTextMesh;
    protected int LetterA;
    protected int LetterB;
    protected int RuleA;
    protected int RuleB;
    protected int Rule;
    protected int RandomFaultRule;
    protected int FaultButton;
    private Color orange = new Color(1, 0.5f, 0);

    int[,] BGmanualTable = new int[6, 6]{
                 { 3, 2, 9, 1, 7, 4 },
                 { 7, 9, 8, 8, 2, 3 },
                 { 5, 1, 7, 4, 4, 6 },
                 { 6, 4, 2, 6, 8, 5 },
                 { 5, 1, 5, 3, 9, 9 },
                 { 1, 2, 3, 6, 7, 8 },
            };

    protected bool SOLVED;
    protected int GoalPresses;
    protected int Presses;

    private string TwitchHelpMessage = "Use \'!{0} press left 5\' to press the left \"Push Me\" button 5 times, Use \'!{0} press right 5\' to press the right \"Push Me\" button 5 times, Use \'!{0} Submit\' to Submit the current answer. Note: Only takes numbers 1-9";

    protected KMSelectable[] ProcessTwitchCommand(string TPInput)
    {
        string lowertpinput = TPInput.ToLowerInvariant();
        bool Incomp = false;
        int TPcurPresses = 0;
        List<KMSelectable> Moves = new List<KMSelectable>();

        string[] split = lowertpinput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        int TPGoalPresses;

        if (split.Length == 3 && split[0] == "press" && split[1] == "left" && int.TryParse(split[2], out TPGoalPresses))
        {
            while (TPcurPresses != TPGoalPresses)
            {
                Moves.Add(ButtonA);
                TPcurPresses++;
            }
        }
        else if (split.Length == 3 && split[0] == "press" && split[1] == "right" && int.TryParse(split[2], out TPGoalPresses))
        {
            while (TPcurPresses != TPGoalPresses)
            {
                Moves.Add(ButtonB);
                TPcurPresses++;
            }
        }
        else if (lowertpinput == "submit")
        {
            Moves.Add(Submit);
        }
        else
        {
            Incomp = true;
        }

        if (Incomp == false)
        {
            KMSelectable[] MovesArray = Moves.ToArray();
            return MovesArray;
        }
        else
        {
            return null;
        }
    }

    protected void Start()
    {
        //set the faulty button
        RandomFaultRule = UnityEngine.Random.Range(1, 10);
        FBackgrounds_moduleId = FBackgrounds_moduleIdCounter++;
        ColBacking = UnityEngine.Random.Range(1, 9);
        ColButton = UnityEngine.Random.Range(1, 10);
        int ColFault = UnityEngine.Random.Range(1, 10);

        while (ColFault == ColBacking || ColFault == ColButton)
        {
            ColFault++;
            if (ColFault > 9)
            {
                ColFault = 1;
            }
        }

        Submit.OnInteract += HandlePressSubmit;

        if (ColButton == ColBacking)
        {
            ButtonA.OnInteract += HandlePressButton;
            ButtonB.OnInteract += HandlePressButton;
            FaultButton = UnityEngine.Random.Range(1, 3);
        }
        else
        {
            if (RandomFaultRule != 1)
            {
                ButtonA.OnInteract += HandlePressButton;
                ButtonB.OnInteract += HandlePressButton;

                if (RandomFaultRule == 2)
                {
                    FaultButton = 1;
                    int FaultText = UnityEngine.Random.Range(1, 3);
                    if (FaultText == 1)
                    {
                        ButtonATextMesh.text = "BUSH\nME!";
                    }
                    else
                    {
                        ButtonBTextMesh.text = "BUSH\nME!";
                    }
                }
                else if (RandomFaultRule == 3)
                {
                    FaultButton = 2;
                    int FaultText = UnityEngine.Random.Range(1, 3);
                    if (FaultText == 1)
                    {
                        ButtonATextMesh.text = "PUSH\nNE!";
                    }
                    else
                    {
                        ButtonBTextMesh.text = "PUSH\nNE!";
                    }
                }
                else if (RandomFaultRule == 4)
                {
                    int FaultText = UnityEngine.Random.Range(1, 3);
                    if (FaultText == 1)
                    {
                        ButtonATextMesh.text = "PUSH\nHE!";
                        FaultButton = 1;
                    }
                    else
                    {
                        ButtonBTextMesh.text = "PUSH\nHE!";
                        FaultButton = 2;
                    }
                }
                else if (RandomFaultRule == 5)
                {
                    int FaultText = UnityEngine.Random.Range(1, 3);
                    if (FaultText == 1)
                    {
                        ButtonATextMesh.text = "PUSH\nSHE!";
                        FaultButton = 2;
                    }
                    else
                    {
                        ButtonBTextMesh.text = "PUSH\nSHE!";
                        FaultButton = 1;
                    }
                }
                else if (RandomFaultRule == 6)
                {
                    FaultButton = 1;
                    CounterText.color = Color.black;
                }
                else if (RandomFaultRule == 7)
                {
                    FaultButton = 2;
                }
                else if (RandomFaultRule == 8)
                {
                    if (ColFault != 9)
                    {
                        RandomFaultRule = 9;
                    }
                    else
                    {
                        FaultButton = UnityEngine.Random.Range(1, 3);
                    }
                }
                if (RandomFaultRule == 9)
                {
                    if (BombInfo.GetSerialNumber().Last() % 2 == 0)
                    {
                        FaultButton = 2;
                    }
                    else
                    {
                        FaultButton = 1;
                        RandomFaultRule = 10;
                    }
                }
            }
            else
            {
                RandomFaultRule = 0;
                FaultButton = UnityEngine.Random.Range(1, 3);
                if (FaultButton == 1)
                {
                    ButtonB.OnInteract += HandlePressButton;
                }
                else
                {
                    ButtonA.OnInteract += HandlePressButton;
                }
            }
        }


        string ColButtonName = "";
        string ColBackingName = "";
        string ColFaultName = "";

        if (ColBacking == 1)
        {
            BackingMesh.material.color = Color.red;
            ColBackingName = "red";
        }
        if (ColBacking == 2)
        {
            BackingMesh.material.color = orange;
            ColBackingName = "orange";
        }
        if (ColBacking == 3)
        {
            BackingMesh.material.color = Color.yellow;
            ColBackingName = "yellow";
        }
        if (ColBacking == 4)
        {
            BackingMesh.material.color = Color.green;
            ColBackingName = "green";
        }
        if (ColBacking == 5)
        {
            BackingMesh.material.color = Color.blue;
            ColBackingName = "blue";
        }
        if (ColBacking == 6)
        {
            BackingMesh.material.color = Color.magenta;
            ColBackingName = "purple";
        }
        if (ColBacking == 7)
        {
            BackingMesh.material.color = Color.white;
            ColBackingName = "white";
        }
        if (ColBacking == 8)
        {
            BackingMesh.material.color = Color.grey;
            ColBackingName = "grey";
        }


        if (ColButton == 1)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.red;
            }
            else
            {
                ButtonBMesh.material.color = Color.red;
            }
            ColButtonName = "red";
        }
        if (ColButton == 2)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = orange;
            }
            else
            {
                ButtonBMesh.material.color = orange;
            }
            ColButtonName = "orange";
        }
        if (ColButton == 3)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.yellow;
            }
            else
            {
                ButtonBMesh.material.color = Color.yellow;
            }
            ColButtonName = "yellow";
        }
        if (ColButton == 4)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.green;
            }
            else
            {
                ButtonBMesh.material.color = Color.green;
            }
            ColButtonName = "green";
        }
        if (ColButton == 5)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.blue;
                ButtonATextMesh.color = Color.white;
            }
            else
            {
                ButtonBMesh.material.color = Color.blue;
                ButtonBTextMesh.color = Color.white;
            }
            ColButtonName = "blue";
        }
        if (ColButton == 6)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.magenta;
            }
            else
            {
                ButtonBMesh.material.color = Color.magenta;
            }
            ColButtonName = "purple";
        }
        if (ColButton == 7)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.white;
            }
            else
            {
                ButtonBMesh.material.color = Color.white;
            }
            ColButtonName = "white";

        }
        if (ColButton == 8)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.grey;
            }
            else
            {
                ButtonBMesh.material.color = Color.grey;
            }
            ColButtonName = "grey";
        }
        if (ColButton == 9)
        {
            if (FaultButton == 2)
            {
                ButtonAMesh.material.color = Color.black;
                ButtonATextMesh.color = Color.white;
            }
            else
            {
                ButtonBMesh.material.color = Color.black;
                ButtonBTextMesh.color = Color.white;
            }
            ColButtonName = "black";
        }


        if (ColFault == 1)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.red;
            }
            else
            {
                ButtonAMesh.material.color = Color.red;
            }
            ColFaultName = "red";
        }
        if (ColFault == 2)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = orange;
            }
            else
            {
                ButtonAMesh.material.color = orange;
            }
            ColFaultName = "orange";
        }
        if (ColFault == 3)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.yellow;
            }
            else
            {
                ButtonAMesh.material.color = Color.yellow;
            }
            ColFaultName = "yellow";
        }
        if (ColFault == 4)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.green;
            }
            else
            {
                ButtonAMesh.material.color = Color.green;
            }
            ColFaultName = "green";
        }
        if (ColFault == 5)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.blue;
                ButtonBTextMesh.color = Color.white;
            }
            else
            {
                ButtonAMesh.material.color = Color.blue;
                ButtonATextMesh.color = Color.white;
            }
            ColFaultName = "blue";
        }
        if (ColFault == 6)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.magenta;
            }
            else
            {
                ButtonAMesh.material.color = Color.magenta;
            }
            ColFaultName = "purple";
        }
        if (ColFault == 7)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.white;
            }
            else
            {
                ButtonAMesh.material.color = Color.white;
            }
            ColFaultName = "white";

        }
        if (ColFault == 8)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.grey;
            }
            else
            {
                ButtonAMesh.material.color = Color.grey;
            }
            ColFaultName = "grey";
        }
        if (ColFault == 9)
        {
            if (FaultButton == 2)
            {
                ButtonBMesh.material.color = Color.black;
                ButtonBTextMesh.color = Color.white;
            }
            else
            {
                ButtonAMesh.material.color = Color.black;
                ButtonATextMesh.color = Color.white;
            }
            ColFaultName = "black";
        }

        //Check Categories of button and backing where necessary
        bool BackingPrimary = false;
        bool BackingGreyscale = false;
        bool ButtonPrimary = false;
        bool ButtonSecondary = false;
        bool ButtonGreyscale = false;
        bool Mixrule = false;

        if (ColBacking == 7 || ColBacking == 9)
        {
            BackingGreyscale = true;
        }
        if (ColButton == 7 || ColButton == 9)
        {
            ButtonGreyscale = true;
        }
        if (ColButton == 1 || ColButton == 3 || ColButton == 5)
        {
            ButtonPrimary = true;
        }
        if (ColBacking == 1 || ColBacking == 3 || ColBacking == 5)
        {
            BackingPrimary = true;
        }
        if (ColButton == 2 || ColButton == 4 || ColButton == 6)
        {
            ButtonSecondary = true;
        }

        if (ColBacking == 1 && ColButton == 6)
        {
            Mixrule = true;
        }
        if (ColBacking == 3 && ColButton == 4)
        {
            Mixrule = true;
        }

        //determine letters
        int letterpos = 0;
        if (ColBacking == ColButton)
        {
            if (letterpos == 0)
            {
                LetterA = 1;
                RuleA = 0;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 3;
                RuleB = 0;
                letterpos++;
            }
        }

        if (BackingGreyscale || ButtonGreyscale)
        {
            if (letterpos == 0)
            {
                LetterA = 4;
                RuleA = 1;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 2;
                RuleB = 1;
                letterpos++;
            }
        }

        if (BombInfo.GetBatteryCount(KMBI.KnownBatteryType.D) == 0)
        {
            if (letterpos == 0)
            {
                LetterA = 3;
                RuleA = 2;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 5;
                RuleB = 2;
                letterpos++;
            }
        }
        if (BombInfo.GetBatteryCount(KMBI.KnownBatteryType.AA) < 1)
        {
            if (letterpos == 0)
            {
                LetterA = 4;
                RuleA = 3;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 4;
                RuleB = 3;
                letterpos++;
            }
        }
        if (BackingPrimary && ButtonPrimary)
        {
            if (letterpos == 0)
            {
                LetterA = 2;
                RuleA = 4;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 6;
                RuleB = 4;
                letterpos++;
            }
        }
        if (ButtonSecondary)
        {
            if (letterpos == 0)
            {
                LetterA = 6;
                RuleA = 5;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 5;
                RuleB = 5;
                letterpos++;
            }
        }
        if (BombInfo.GetOffIndicators().Contains("SND"))
        {
            if (letterpos == 0)
            {
                LetterA = 5;
                RuleA = 6;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 2;
                RuleB = 6;
                letterpos++;
            }
        }
        if (BombInfo.GetPorts().Contains("Serial"))
        {
            if (letterpos == 0)
            {
                LetterA = 2;
                RuleA = 7;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 3;
                RuleB = 7;
                letterpos++;
            }
        }
        if (Mixrule)
        {
            if (letterpos == 0)
            {
                LetterA = 3;
                RuleA = 8;
                letterpos++;
            }
            else if (letterpos == 1)
            {
                LetterB = 4;
                RuleB = 8;
                letterpos++;
            }
        }

        if (letterpos == 0)
        {
            LetterA = 5;
            RuleA = 9;
            LetterB = 1;
            RuleB = 9;
        }
        else if (letterpos == 1)
        {
            LetterB = 1;
            RuleB = 9;
        }

        GoalPresses = BGmanualTable[LetterA - 1, LetterB - 1];

        if (FaultButton == 1)
        {
            DebugLog("Fake Button is on the Left");
        }
        else
        {
            DebugLog("Fake Button is on the Right");
        }

        if (ColButton != ColBacking && RandomFaultRule != 1)
        {
            DebugLog("Fake Button was determined by rule {0}", RandomFaultRule + 1);
        }
        else
        {
            if (ColButton == ColBacking)
            {
                DebugLog("Fake Button was determined by rule 2");
            }
            else
            {
                DebugLog("Fake Button was determined by rule 1");
            }
        }


        DebugLog("Backing is {0}, Button is {1}, Fake Button is {2}", ColBackingName, ColButtonName, ColFaultName);
        DebugLog("Column in table is {0} due to rule {1}", (LetterA + 9).ToString("X"), RuleA + 1);
        DebugLog("Row in table is {0} due to rule {1}", (LetterB + 9).ToString("X"), RuleB + 1);
        DebugLog("Number needed is {0}", GoalPresses);
    }

    protected bool HandlePressSubmit()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
        Submit.AddInteractionPunch(0.5f);

        if (!SOLVED)
        {
            DebugLog("Submitted {0}, Goal was {1}", Presses, GoalPresses);
            if (Presses == GoalPresses)
            {
                BombModule.HandlePass();
                SOLVED = false;
                DebugLog("The module has been defused.");
            }
            else
            {
                BombModule.HandleStrike();
            }
        }
        return false;
    }

    protected bool HandlePressButton()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
        ButtonA.AddInteractionPunch(0.5f);

        if (!SOLVED)
        {
            Presses++;
            if (Presses == 10)
            {
                Presses = 0;
            };
            DebugLog("Counter is now {0}", Presses, GoalPresses);
            CounterText.text = Presses.ToString();
            if (ColButton != ColBacking && RandomFaultRule == 6 && Presses%2 == 0)
            {
                CounterText.color = Color.black;

            }
            else if (ColButton != ColBacking && RandomFaultRule == 7 && Presses%2 == 1)
            {
                CounterText.color = Color.black;

            }
            else if (ColButton != ColBacking && RandomFaultRule == 8 && Presses == 5)
            {
                CounterText.color = Color.black;
            }
            else
            {
                CounterText.color = Color.white;
            }
        }
        return false;
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Faulty Backgrounds #{0}] {1}", FBackgrounds_moduleId, logData);
    }
}
