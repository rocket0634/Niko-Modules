using KMBombInfoExtensions;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Backgrounds : MonoBehaviour
{
    public enum Type
    {
        Normal,
        Faulty
    }

    private static int Backgrounds_moduleIdCounter = 1;
    private int Backgrounds_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Submit;
    public KMSelectable ButtonA, ButtonB;
    public MeshRenderer ButtonAMesh, ButtonBMesh, BackingMesh;
    public TextMesh CounterText, ButtonATextMesh, ButtonBTextMesh;
    public Type ModuleType;
    private bool activated;
    private List<bool> check = new List<bool>();
    Color orange = new Color(1, 0.5f, 0);
    protected int LetterA, LetterB, RuleA, RuleB, RandomFaultRule;
    protected int FaultButton, ColBacking, ColButton;
    private readonly Color[] color = { Color.red, new Color(1, 0.5f, 0), Color.yellow, Color.green, Color.blue, Color.magenta, Color.white, Color.grey, Color.black };
    private readonly string[] colorList = { "red", "orange", "yellow", "green", "blue", "purple", "white", "gray", "black" };

    private readonly int[] list1 = { 0, 3, 2, 3, 1, 5, 4, 1, 2, 4 };
    private readonly int[] list2 = { 2, 1, 4, 3, 5, 4, 1, 2, 3, 0 };

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

    int GetSolvedCount()
    {
        return BombInfo.GetSolvedModuleNames().Count;
    }

    protected void Start()
    {
        Backgrounds_moduleId = Backgrounds_moduleIdCounter++;
        ColBacking = UnityEngine.Random.Range(0, 8);
        ColButton = UnityEngine.Random.Range(0, 9);

        Submit.OnInteract += HandlePressSubmit;

        //Add each rule into a boolean list
        //Same color rule
        check.Add(ColButton == ColBacking);
        //Greyscale rule
        check.Add(ColBacking == 6 || ColButton == 6 || ColButton == 8);
        //No D Battery rule
        check.Add(BombInfo.GetBatteryCount(KMBI.KnownBatteryType.D) == 0);
        //No AA Battery rule
        check.Add(BombInfo.GetBatteryCount(KMBI.KnownBatteryType.AA) == 0);
        //Primary colors rule
        check.Add((ColButton == 0 || ColButton == 2 || ColButton == 4) && (ColBacking == 0 || ColBacking == 2 || ColBacking == 4));
        //Secondary colors rule
        check.Add(ColButton == 1 || ColButton == 3 || ColButton == 5);
        //Unlit SND rule
        check.Add(BombInfo.GetOffIndicators().Contains("SND"));
        //Serial port rule
        check.Add(BombInfo.GetPorts().Contains("Serial"));
        //Mix with blue rule
        check.Add((ColBacking == 0 && ColButton == 5) || (ColBacking == 2 && ColButton == 3));
        //Otherwise rule, always true
        check.Add(true);

        var correctMesh = new MeshRenderer();
        var correctTextMesh = new TextMesh();

        if (ModuleType.Equals(Type.Normal))
        {
            ButtonA.OnInteract += delegate () { HandlePressButton(ButtonA); return false; };
            correctMesh = ButtonAMesh;
            correctTextMesh = ButtonATextMesh;
            DebugLog("Backing is {0}, Button is {1}", colorList[ColBacking], colorList[ColButton]);
        }
        if (ModuleType.Equals(Type.Faulty))
        {
            RandomFaultRule = UnityEngine.Random.Range(0, 10);
            if (RandomFaultRule > 0) RandomFaultRule += 1;
            if (check[0]) RandomFaultRule = 1;
            FaultButton = UnityEngine.Random.Range(0, 2);
            var faultButton = new [] { ButtonATextMesh, ButtonBTextMesh };
            var faultText = UnityEngine.Random.Range(0, 2);
            var faultyMesh = new MeshRenderer();
            var faultyTextMesh = new TextMesh();
            int colFault = UnityEngine.Random.Range(0, 9);
            while (colFault == ColBacking || colFault == ColButton)
            {
                colFault = UnityEngine.Random.Range(0, 9);
            }
            switch (RandomFaultRule)
            {
                case 0:
                    if (FaultButton == 0) ButtonB.OnInteract += delegate () { HandlePressButton(ButtonB); return false; };
                    else ButtonA.OnInteract += delegate () { HandlePressButton(ButtonA); return false; };
                    goto broke;
                case 2:
                    FaultButton = 0;
                    faultButton[faultText].text = "BUSH\nME!";
                    break;
                case 3:
                    FaultButton = 1;
                    faultButton[faultText].text = "PUSH\nNE!";
                    break;
                case 4:
                    faultText = FaultButton;
                    faultButton[faultText].text = "PUSH\nHE!";
                    break;
                case 5:
                    if (FaultButton == faultText) faultText = (faultText + 1) % 2;
                    faultButton[faultText].text = "PUSH\nSHE!";
                    break;
                case 6:
                    FaultButton = 0;
                    CounterText.color = Color.black;
                    break;
                case 7:
                    FaultButton = 1;
                    break;
                case 8:
                    if (colFault != 9) goto case 9;
                    break;
                case 9:
                    RandomFaultRule = 9;
                    if (BombInfo.GetSerialNumber().Last() % 2 == 0) FaultButton = 1;
                    else goto default;
                    break;
                default:
                    FaultButton = 0;
                    break;
            }
            ButtonA.OnInteract += delegate () { HandlePressButton(ButtonA); return false; };
            ButtonB.OnInteract += delegate () { HandlePressButton(ButtonB); return false; };
            broke:
            if (FaultButton == 0)
            {
                faultyTextMesh = ButtonATextMesh;
                faultyMesh = ButtonAMesh;
                correctTextMesh = ButtonBTextMesh;
                correctMesh = ButtonBMesh;
                DebugLog("Fake Button is on the Left");
            }
            else
            {
                faultyTextMesh = ButtonBTextMesh;
                faultyMesh = ButtonBMesh;
                correctTextMesh = ButtonATextMesh;
                correctMesh = ButtonAMesh;
                DebugLog("Fake Button is on the Right");
            }
            if (colFault == 4 || colFault == 8) faultyTextMesh.color = Color.white;
            faultyMesh.material.color = color[colFault];
            DebugLog("Fake Button was determined by rule {0}", RandomFaultRule + 1);
            DebugLog("Backing is {0}, Button is {1}, Fake Button is {2}", colorList[ColBacking], colorList[ColButton], colorList[colFault]);
        }
        
        BackingMesh.material.color = color[ColBacking];
        correctMesh.material.color = color[ColButton];

        if (ColButton == 4 || ColButton == 8) correctTextMesh.color = Color.white;

        //Grab our X value for the table (the first instance of "true")
        RuleA = check.IndexOf(true);
        //Remove RuleA from check
        check[RuleA] = false;
        //If there are no true values left, it means the otherwise rule was reached twice
        if (!check.Contains(true)) RuleB = RuleA;
        //Otherwise, there's still another true value in check
        else RuleB = check.IndexOf(true);

        //list1 contains X values for the first letter,
        //and list 2 contains Y values for the second letter
        GoalPresses = BGmanualTable[list1[RuleA], list2[RuleB]];

        //ToString("X") translates the number into a hexidecimal.
        //As it so happens, the columns use letters A-F, which fits perfectly into hexidecimal values.
        DebugLog("Row in table is {0} due to rule {1}", (list1[RuleA] + 10).ToString("X"), RuleA + 1);
        DebugLog("Column in table is {0} due to rule {1}", (list2[RuleB] + 10).ToString("X"), RuleB + 1);
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

    void HandlePressButton(KMSelectable Button)
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
            if (ModuleType.Equals(Type.Faulty))
            {
                switch (RandomFaultRule)
                {
                    case 6:
                        if (Presses % 2 == 0)
                        {
                            CounterText.color = Color.black;
                            break;
                        }
                        goto default;
                    case 7:
                        if (Presses % 2 == 1)
                        {
                            CounterText.color = Color.black;
                            break;
                        }
                        goto default;
                    case 8:
                        if (Presses == 5)
                        {
                            CounterText.color = Color.black;
                            break;
                        }
                        goto default;
                    default:
                        CounterText.color = Color.white;
                        break;
                }
            }
        }
        return;
    }

    protected bool HandleLightsToggle()
    {
        return false;
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        if (ModuleType == Type.Normal) Debug.LogFormat("[Backgrounds #{0}] {1}", Backgrounds_moduleId, logData);
        else Debug.LogFormat("[Faulty Backgrounds #{0}] {1}", Backgrounds_moduleId, logData);
    }
}