using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RuleGenerator;

public class Backgrounds : MonoBehaviour
{
    //The code is shared between both Backgrounds and Faulty Backgrounds
    //Let the code know which module is being used
    public enum Type
    {
        Normal,
        Faulty
    }

    private static int Backgrounds_moduleIdCounter = 1;
    private int Backgrounds_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMRuleSeedable RuleSeed;
    public KMAudio KMAudio;
    public KMSelectable Submit;
    public KMSelectable ButtonA, ButtonB;
    public MeshRenderer ButtonAMesh, ButtonBMesh, BackingMesh;
    public TextMesh CounterText, ButtonATextMesh, ButtonBTextMesh;
    public Type ModuleType;
    private bool activated;
    internal List<bool> check;
    internal MeshRenderer correctMesh;
    internal TextMesh correctTextMesh;
    public static Color orange = new Color(1, 0.5f, 0), purple = new Color(0.5f, 0, 0.5f);
    protected internal int LetterA, LetterB, RuleA, RuleB,
        RandomFaultRule;
    internal Color[] color = { Color.red, orange, Color.yellow, Color.green, Color.blue, purple, Color.white, Color.grey, Color.black };
    internal string[] colorList = { "red", "orange", "yellow", "green", "blue", "purple", "white", "gray", "black" };

    internal int[] coordX, coordY;
    internal int[,] BGManualTable = new int[6,6];

    protected bool SOLVED;
    protected int GoalPresses;
    protected int Presses;
    
    public string TwitchHelpMessage = "Use \'!{0} press 5\' to press the \"Push Me\" button 5 times, Use \'!{0} Submit\' to Submit the current answer. Note: Only takes numbers 1-9";

    protected KMSelectable[] ProcessTwitchCommand(string TPInput)
    {
        var split = TPInput.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        int TPcurPresses = 0;
        List<KMSelectable> Moves = new List<KMSelectable>();
        KMSelectable Button;
        int TPGoalPresses;
        bool faulty = ModuleType.Equals(Type.Faulty);
        bool submit = false;
        var Match = Regex.Match(TPInput, @"^\s*(?:press |submit )([0-9])(?: submit|)\s*$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        var FaultyMatch = Regex.Match(TPInput, @"^\s*(?:press |submit )(?:left |right |l |r)([0-9])(?: submit|)\s*$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        if (FaultyMatch.Success && !faulty) throw new FormatException("This command is invalid for Backgrounds.");
        if (Match.Success && faulty) throw new FormatException("Please indicate which button you are interacting with.");

        if (TPInput.ToLowerInvariant() == "submit") return new[] { Submit };

        if (split.Last().Equals("submit"))
        {
            submit = true;
            if (split.Count() > 1) split = split.Take(split.Count() - 1).ToArray();
        }
        switch (split.Count())
        {
            case 3:
                if (!faulty || !FaultyMatch.Success || !int.TryParse(split[2], out TPGoalPresses)) return null;
                if (split[1].Equals("left") || split[1].Equals("l")) Button = ButtonA;
                else if (split[1].Equals("right") || split[1].Equals("r")) Button = ButtonB;
                else return null;
                break;
            case 2:
                if ((faulty && !split[0].Equals("submit")) || !Match.Success || !int.TryParse(split[1], out TPGoalPresses)) return null;
                Button = ButtonA;
                if (faulty)
                {
                    if (split[1].Equals("left") || split[1].Equals("l")) Button = ButtonA;
                    else if (split[1].Equals("right") || split[1].Equals("r")) Button = ButtonB;
                }
                break;
            case 1:
                if (faulty || !submit) return null;
                TPcurPresses = int.Parse(CounterText.text);
                TPGoalPresses = TPcurPresses;
                Button = ButtonA;
                break;
            default:
                return null;
        }
        if (split[0].Equals("submit") && !submit)
        {
            if (int.Parse(CounterText.text) != TPGoalPresses)
            {
                if (int.Parse(CounterText.text) > TPGoalPresses) TPcurPresses = 10 - (int.Parse(CounterText.text) - TPGoalPresses);
                else if (TPGoalPresses < 10) TPcurPresses = TPGoalPresses - int.Parse(CounterText.text);
            }
            return Enumerable.Repeat(Button, TPcurPresses).Concat(new[] { Submit }).ToArray();
        }
        else
        {
            if (split[0].Equals("submit")) return null;
            while (TPcurPresses != TPGoalPresses)
            {
                Moves.Add(Button);
                TPcurPresses++;
            }
            if (submit) Moves.Add(Submit);
            return Moves.ToArray();
        }

    }

    protected void Start()
    {
        BackgroundsRuleGenerator.Module = this;
        Backgrounds_moduleId = Backgrounds_moduleIdCounter++;

        Submit.OnInteract += HandlePressSubmit;
        
        check = BackgroundsRuleGenerator.Rules(RuleSeed.GetRNG());
        //The colors are now determined in the Rule Generator
        //This is due to the fact that all boolean values are determined at runtime
        //So these need to have values before the rules are randomized.
        var colBacking = BackgroundsRuleGenerator.ColBacking;
        var colButton = BackgroundsRuleGenerator.ColButton;

        if (ModuleType == Type.Faulty) Faulty.Rules();
        else
        {
            ButtonA.OnInteract += delegate () { HandlePressButton(ButtonA); return false; };
            correctMesh = ButtonAMesh;
            correctTextMesh = ButtonATextMesh;
            DebugLog("Backing is {0}, Button is {1}", colorList[colBacking], colorList[colButton]);
        }
        
        BackingMesh.material.color = color[colBacking];
        correctMesh.material.color = color[colButton];

        Faulty.ReadableText(colButton, correctTextMesh);

        //Grab our X value for the table (the first instance of "true")
        RuleA = check.IndexOf(true);
        //Remove RuleA from check
        check[RuleA] = false;
        //If there are no true values left, it means the otherwise rule was reached twice
        if (!check.Contains(true)) RuleB = RuleA;
        //Otherwise, there's still another true value in check
        else RuleB = check.IndexOf(true);

        //coordX contains X values for the first letter,
        //and coordY contains Y values for the second letter
        GoalPresses = BGManualTable[coordX[RuleA], coordY[RuleB]];

        //ToString("X") translates the number into a hexidecimal.
        //As it so happens, the columns use letters A-F, which fits perfectly into hexidecimal values.
        DebugLog("Row in table is {0} due to rule {1}", (coordX[RuleA] + 10).ToString("X"), RuleA + 1);
        DebugLog("Column in table is {0} due to rule {1}", (coordY[RuleB] + 10).ToString("X"), RuleB + 1);
        DebugLog("Number needed is {0}", GoalPresses);
        BombModule.OnActivate += delegate { activated = true; };
    }

    protected bool HandlePressSubmit()
    {
        if (!activated) return false;
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
        Submit.AddInteractionPunch(0.5f);

        if (!SOLVED)
        {
            DebugLog("Submitted {0}, Goal was {1}", Presses, GoalPresses);
            if (Presses == GoalPresses)
            {
                BombModule.HandlePass();
                SOLVED = true;
                DebugLog("The module has been defused.");
            }
            else
            {
                BombModule.HandleStrike();
            }
        }
        return false;
    }

    internal void HandlePressButton(KMSelectable Button)
    {
        if (!activated) return;
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
        Button.AddInteractionPunch(0.5f);

        if (!SOLVED) {
            Presses++;
            if (Presses == 10)
            {
                Presses = 0;
            };

            CounterText.text = Presses.ToString();
            //Handles rules where the digits on the screen occasionally disappear
            if (ModuleType.Equals(Type.Faulty))
            {
                var presses = new[] { Presses % 2 == 0, Presses % 2 == 1, Presses == 5 };
                var hold = new[] { RandomFaultRule == 6, RandomFaultRule == 7, RandomFaultRule == 8 };
                var index = Array.IndexOf(hold, true);
                if (index == -1) return;
                if (presses[index])
                {
                    CounterText.color = Color.black;
                }
                else CounterText.color = Color.white;
            }
        }
    }

    public void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        if (ModuleType == Type.Normal) Debug.LogFormat("[Backgrounds #{0}] {1}", Backgrounds_moduleId, logData);
        else Debug.LogFormat("[Faulty Backgrounds #{0}] {1}", Backgrounds_moduleId, logData);
    }
}