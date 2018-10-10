using UnityEngine;
using System.Linq;
using KModkit;
using BackgroundsRuleGenerator;

public class Faulty
{
    //Grab instance of Module for convenience
    public Backgrounds Module;
    //The color of the faulty button
    private int colFault = Random.Range(0, 9);
    //Which button is faulty
    private int fault;
    //This is only in Backgrounds.cs due to the disappearing text rule.
    private int RandomFaultRule
    {
        get
        {
            return Module.RandomFaultRule;
        }
        set
        {
            Module.RandomFaultRule = value;
        }
    }
    //This is used to change the text of the faulty button.
    //Although it's also used to determine which button is not faulty.
    private TextMesh[] FaultyTextMesh
    {
        get
        {
            return new[] { Module.ButtonATextMesh, Module.ButtonBTextMesh };
        }
    }
    //This is used to change the color of the buttons
    private MeshRenderer[] FaultyMesh
    {
        get
        {
            return new[] { Module.ButtonAMesh, Module.ButtonBMesh };
        }
    }
    //The color of the button and backing, determined in BackgroundsRuleGenerator
    private int ColButton { get { return Module.Rules.ColButton; } }
    private int ColBacking { get { return Module.Rules.ColBacking; } }

    internal void Rules()
    {
        RandomFaultRule = Random.Range(0, 9);
        fault = Random.Range(0, 2);
        //The faulty button cannot be the same color as the correct button
        //FaultRule 2 also states that the correct button has to be the one that matches the backing's color
        while (colFault.Equals(ColButton) || colFault.Equals(ColBacking))
            colFault = Random.Range(0, 9);

        //This determines which button is the faulty button, based on the rules.
        //While "fault" is always randomized, it doesn't necessarily mean the faulty button itself is always randomized.
        //This is why some values are fault, while others are actual integers, or the opposite of fault.
        var values = new[]
        {
            fault, fault, 0, 1, fault, (fault + 1) % 2, 0, 1, fault, 1, 0
        };
        
        //FaultRule 1 is excluded due to the OnInteract rule
        //FaultRule 2 is included as it overrides FaultRule 1
        if (RandomFaultRule != 0 || (ColButton == ColBacking))
        {
            //FaultRule 2 is determined by rather or not the button is the same color as the backing
            //This means it cannot be randomized based on the rule itself, at least not easily.
            if (ColButton == ColBacking) RandomFaultRule = 1;
            //Since FaultRule 2 is in the middle of the random function, increase the RandomFaultRule by 1 to match the manual
            //FaultRule 1 does not need to be increased
            else RandomFaultRule++;
            Module.ButtonA.OnInteract += delegate { Module.HandlePressButton(Module.ButtonA); return false; };
            Module.ButtonB.OnInteract += delegate { Module.HandlePressButton(Module.ButtonB); return false; };
        }
        else
        {
            //FaultRule 1 should be the only rule that reaches this else statement
            if (fault == 0) Module.ButtonB.OnInteract += delegate () { Module.HandlePressButton(Module.ButtonB); return false; };
            else Module.ButtonA.OnInteract += delegate () { Module.HandlePressButton(Module.ButtonA); return false; };
        }
        
        //Check each rule until something returns true
        //This is mostly used for FaultRule 9, 10 and 11, since they're not randomized
        while (!CheckRules())
        {
            RandomFaultRule++;
        }
        //Determine which button is faulty. 0 is left, 1 is right
        fault = values[RandomFaultRule];

        //Grab the correct button and text so that Backgrounds.cs can apply their correct colors
        Module.correctMesh = FaultyMesh[(fault + 1) % 2];
        Module.correctTextMesh = FaultyTextMesh[(fault + 1) % 2];
        Module.DebugLog("Fake Button is on the {0}", fault.Equals(0) ? "left" : "right");
        FaultyMesh[fault].material.color = Backgrounds.color[colFault];

        Module.DebugLog("Fake Button was determined by rule {0}", RandomFaultRule + 1);
        Module.DebugLog("Backing is {0}, Button is {1}, Fake Button is {2}", Backgrounds.colorList[ColBacking], Backgrounds.colorList[ColButton], Backgrounds.colorList[colFault]);
        ReadableText(colFault, FaultyTextMesh[fault]);
    }

    //Change the text's color to white for text that may be difficult to read
    //This method could really either be here or in Backgrounds.cs
    //But since all of the static methods are in here, just leave it here.
    public static void ReadableText(int a, TextMesh mesh)
    {
        if (a.Equals(4) || a.Equals(8)) mesh.color = Color.white;
    }

    public bool CheckRules()
    {
        //Here's the problem with using boolean arrays/lists
        //They don't actually save what's being compared
        //So, we need to use an object array/list instead.
        var rules = new object[]
        {
            null, ColButton == ColBacking, "BUSH\nME!", "PUSH\nNE!", "PUSH\nHE!", "PUSH\nSHE!", Color.black, null,
            ColButton != 8, Module.BombInfo.GetSerialNumber().Last() % 2 == 0, null
        };
        //Grab the necessary object from whatever rule has been chosen
        var i = RandomFaultRule;
        //Use these to change the text of the buttons, or to make the digit on the screen disappear
        //Or to determine the last three rules
        return rules[i] is bool ? IsText((bool)rules[i]) : 
               rules[i] is string ? IsText((string)rules[i]) : 
               rules[i] is Color ? IsText((Color)rules[i]) :
               true;
    }

    public bool IsText(string compare)
    {
        //Sets the text for rules that change the button's text
        FaultyTextMesh[fault].text = compare;
        return true;
    }

    public bool IsText(Color color)
    {
        //Change the color of the digit on the screen to be black
        //Only applicable when the module is loaded, the rest is handled in Backgrounds.cs
        Module.CounterText.color = color;
        return true;
    }

    public bool IsText(bool compare)
    {
        //If colFault isn't already 8, set it to 8 if RandomFaultRule is also 8
        //This should make FaultRule 9 more common
        colFault = (RandomFaultRule == 8 && compare) ? RandomFaultRule : colFault;
        return compare;
    }
}