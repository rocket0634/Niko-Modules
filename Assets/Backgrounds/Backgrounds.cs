using KMBombInfoExtensions;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Backgrounds : MonoBehaviour
{
    private static int Backgrounds_moduleIdCounter = 1;
    private int Backgrounds_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Submit;
    public KMSelectable Button;
    public MeshRenderer ButtonMesh;
    public MeshRenderer BackingMesh;
    public TextMesh CounterText;
    public TextMesh ButtonTextMesh;
    Color orange = new Color(1, 0.5f, 0);
    protected int LetterA;
    protected int LetterB;
    protected int RuleA;
    protected int RuleB;

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

    public string TwitchHelpMessage = "Use \'!{0} press 5\' to press the \"Push Me\" button 5 times, Use \'!{0} Submit\' to Submit the current answer. Note: Only takes numbers 1-9";

    protected KMSelectable[] ProcessTwitchCommand(string TPInput)
    {
        string lowertpinput = TPInput.ToLowerInvariant();
        bool Incomp = false;
        int TPcurPresses = 0;
        List<KMSelectable> Moves = new List<KMSelectable>();

        string[] split = lowertpinput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        int TPGoalPresses;

        if (split.Length == 2 && split[0] == "press" && int.TryParse(split[1], out TPGoalPresses))
        {
            while (TPcurPresses != TPGoalPresses)
            {
                Moves.Add(Button);
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

    int GetSolvedCount()
    {
        return BombInfo.GetSolvedModuleNames().Count;
    }

    protected void Start()
    {
        Backgrounds_moduleId = Backgrounds_moduleIdCounter++;
        int ColBacking = UnityEngine.Random.Range(1, 9);
        int ColButton = UnityEngine.Random.Range(1, 10);

        Button.OnInteract += HandlePressButton;
        Submit.OnInteract += HandlePressSubmit;

        //Determine Values of the Knobs and Color the knobs 1RED 2GREEN 3WHITE 4GREY 5 YELLOW
        string ColButtonName = "";
        string ColBackingName = "";
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
            ButtonMesh.material.color = Color.red;
            ColButtonName = "red";
        }
        if (ColButton == 2)
        {
           ButtonMesh.material.color = orange;
           ColButtonName = "orange";
        }
        if (ColButton == 3)
        {
            ButtonMesh.material.color = Color.yellow;
            ColButtonName = "yellow";
        }
        if (ColButton == 4)
        {
            ButtonMesh.material.color = Color.green;
            ColButtonName = "green";
        }
        if (ColButton == 5)
        {
            ButtonMesh.material.color = Color.blue;
            ButtonTextMesh.color = Color.white;
            ColButtonName = "blue";
        }
        if (ColButton == 6)
        {
            ButtonMesh.material.color = Color.magenta;
            ColButtonName = "purple";
        }
        if (ColButton == 7)
        {
            ButtonMesh.material.color = Color.white;
            ColButtonName = "white";

        }
        if (ColButton == 8)
        {
            ButtonMesh.material.color = Color.grey;
            ColButtonName = "grey";
        }
        if (ColButton == 9)
        {
            ButtonMesh.material.color = Color.black;
            ButtonTextMesh.color = Color.white;
            ColButtonName = "black";
        }

        //Check Categories of button and backing where necessary
        bool BackingPrimary = false;
        bool BackingGreyscale = false;
        bool ButtonPrimary = false;
        bool ButtonSecondary = false;
        bool ButtonGreyscale = false;
        bool Mixrule = false;

        if (ColBacking == 7 || ColBacking == 8 || ColBacking == 9)
        {
            BackingGreyscale = true;
        }
        if (ColButton == 7 || ColButton == 8 || ColButton == 9)
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

        if (BombInfo.GetBatteryCount() == 0)
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
        if (BombInfo.GetBatteryCount(KMBI.KnownBatteryType.AA) > 1)
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


        DebugLog("Backing is {0}, Button is {1}", ColBackingName, ColButtonName);
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
        Button.AddInteractionPunch(0.5f);

        if (!SOLVED)
        {
            Presses++;
            if (Presses == 10)
            {
                Presses = 0;
            };
            CounterText.text = Presses.ToString();
        }
        return false;
    }

    protected bool HandleLightsToggle()
    {
        return false;
    }



    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Backgrounds #{0}] {1}", Backgrounds_moduleId, logData);
    }
}