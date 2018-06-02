using KMBombInfoExtensions;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class FontSelect : MonoBehaviour
{
    private static int FontSelect_moduleIdCounter = 1;
    private int FontSelect_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Left;
    public KMSelectable Right;
    public KMSelectable Submit;
    public TextMesh TextMesh;
    public Material MMerriweather;
    public Material MSpecialElite;
    public Material MRockSalt;
    public Material MChewy;
    public Material MKarma;
    public Material MLobster;
    public Material MComingSoon;
    public Material MGochiHand;
    public Material MIndieFlower;

    public Font Merriweather;
    public Font SpecialElite;
    public Font RockSalt;
    public Font Chewy;
    public Font Karma;
    public Font Lobster;
    public Font ComingSoon;
    public Font GochiHand;
    public Font IndieFlower;

    protected bool Solved;
    protected int PhraseNumber;
    protected int FirstFont;
    protected int SecondFont;
    protected int ThirdFont;
    protected int CurrentFont = 1;
    protected bool FontSelected;
    private Material[] FontMList = { null, null, null, null, null, null, null, null, null };
    private Font[] FontList = { null, null, null, null, null, null, null, null, null };

    public string TwitchHelpMessage = "Use !{0} Left or !{0} Right to cycle fonts, !{0} Submit to submit the current font.";

    protected KMSelectable[] ProcessTwitchCommand(string TPInput)
    {
        string tpinput = TPInput.ToLowerInvariant();
        bool Incomp = false;
        List<KMSelectable> Moves = new List<KMSelectable>();
        if (tpinput == "submit")
        {
            Moves.Add(Submit);
        }
        else if (tpinput == "right")
        {
            Moves.Add(Right);
        }
        else if (tpinput == "left")
        {
            Moves.Add(Left);
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
        FontSelect_moduleId = FontSelect_moduleIdCounter++;

        PhraseNumber = UnityEngine.Random.Range(0, 16);
        FirstFont = UnityEngine.Random.Range(0, 9);
        SecondFont = UnityEngine.Random.Range(0, 9);
        ThirdFont = UnityEngine.Random.Range(0, 9);

        while (SecondFont == FirstFont)
        {
            SecondFont = UnityEngine.Random.Range(0, 9);
        }
        while (ThirdFont == FirstFont || ThirdFont == SecondFont)
        {
            ThirdFont = UnityEngine.Random.Range(0, 9);
        }

        int[] ListNumbers = { 1, 4, 4, 2, 3, 3, 1, 2, 3, 1, 3, 4, 4, 1, 2, 2 };
        string[] ListPhrases = { "Eight Ate\n8", "Jokes on\nyou! I'm\nthe male.", "Jokes on\nyou! I'm\nmale.", "Testing,\ntesting,\n1 to 3.", "Yew R.\nWonn", "Jokes on\nyou! I'm\nthe mail.", "Ewe Arr\nWon", "888", "U.R. 1", "You are\none", "Ate, Ate,\nAte", "8 ate\neight", "Testing,\ntesting, 123", "Testing,\ntesting, 1-3", "Jokes on\nyou! I'm\nfemale.", "Testing,\ntesting,\n1 two 3." };
        int FontListNum = ListNumbers[PhraseNumber];
        string Phrase = ListPhrases[PhraseNumber];
        if (FontListNum == 1)
        {
            FontMList = new Material[] { MSpecialElite, MComingSoon, MIndieFlower, MKarma, MRockSalt, MLobster, MChewy, MMerriweather, MGochiHand };
            FontList = new Font[] { SpecialElite, ComingSoon, IndieFlower, Karma, RockSalt, Lobster, Chewy, Merriweather, GochiHand };
            FontSelected = true;
        }
        else if (FontListNum == 2)
        {
            FontMList = new Material[] { MMerriweather, MChewy, MKarma, MIndieFlower, MRockSalt, MGochiHand, MLobster, MComingSoon, MSpecialElite };
            FontList = new Font[] { Merriweather, Chewy, Karma, IndieFlower, RockSalt, GochiHand, Lobster, ComingSoon, SpecialElite };
            FontSelected = true;
        }
        else if (FontListNum == 3)
        {
            FontMList = new Material[] { MIndieFlower, MComingSoon, MChewy, MMerriweather, MSpecialElite, MLobster, MKarma, MGochiHand, MRockSalt };
            FontList = new Font[] { IndieFlower, ComingSoon, Chewy, Merriweather, SpecialElite, Lobster, Karma, GochiHand, RockSalt };
            FontSelected = true;
        }
        else
        {
            FontMList = new Material[] { MGochiHand, MLobster, MSpecialElite, MRockSalt, MIndieFlower, MMerriweather, MKarma, MComingSoon, MChewy, };
            FontList = new Font[] { GochiHand, Lobster, SpecialElite, RockSalt, IndieFlower, Merriweather, Karma, ComingSoon, Chewy, };
            FontSelected = true;
        }
        TextMesh.text = Phrase;

        Left.OnInteract = HandlePressL;
        Right.OnInteract = HandlePressR;
        Submit.OnInteract = HandlePressSubmit;

        DebugLog("Phrase is \"{1}\" which makes the List Number is {0}", FontListNum, Phrase);
        DebugLog("First Font is \"{0}\", Second Font is \"{1}\", Third Font is \"{2}\"", FontList[FirstFont].name, FontList[SecondFont].name, FontList[ThirdFont].name);
        if (FirstFont < SecondFont && FirstFont < ThirdFont)
        {
            DebugLog("Correct font is the First Font");
        }
        else if (SecondFont < FirstFont && SecondFont < ThirdFont)
        {
            DebugLog("Correct font is the Second Font");
        }
        else if (ThirdFont < SecondFont && ThirdFont < FirstFont)
        {
            DebugLog("Correct font is the Third Font");
        }
    }

    protected bool HandlePressL()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
        Left.AddInteractionPunch(0.5f);

        if (!Solved)
        {
            if (CurrentFont != 1)
            {
                CurrentFont = CurrentFont - 1;
            }
        }
        return false;
    }

    protected bool HandlePressR()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
        Right.AddInteractionPunch(0.5f);

        if (!Solved)
        {
            if (CurrentFont != 3)
            {
                CurrentFont = CurrentFont + 1;
            }
        }
        return false;
    }

    protected bool HandlePressSubmit()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
        Submit.AddInteractionPunch(0.5f);

        if (!Solved)
        {
            if (CurrentFont == 1 && FirstFont < SecondFont && FirstFont < ThirdFont)
            {
                BombModule.HandlePass();
            }
            else if (CurrentFont == 2 && SecondFont < FirstFont && SecondFont < ThirdFont)
            {
                BombModule.HandlePass();
            }
            else if (CurrentFont == 3 && ThirdFont < SecondFont && ThirdFont < FirstFont)
            {
                BombModule.HandlePass();
            }
            else
            {
                BombModule.HandleStrike();
            }
        }

        return false;
    }

    private void Update()
    {
        if (FontSelected == true)
        {
            if (CurrentFont == 1 && TextMesh.GetComponent<Renderer>().material != FontList[FirstFont])
            {
                TextMesh.GetComponent<Renderer>().material = FontMList[FirstFont];
                TextMesh.font = FontList[FirstFont];
                TextMesh.color = Color.cyan;
                if (TextMesh.font == RockSalt)
                {
                    TextMesh.fontSize = 20;
                }
                else
                {
                    TextMesh.fontSize = 30;
                }
            }
            else if (CurrentFont == 2 && TextMesh.GetComponent<Renderer>().material != FontList[SecondFont])
            {
                TextMesh.GetComponent<Renderer>().material = FontMList[SecondFont];
                TextMesh.font = FontList[SecondFont];
                TextMesh.color = Color.cyan;
                if (TextMesh.font == RockSalt)
                {
                    TextMesh.fontSize = 20;
                }
                else
                {
                    TextMesh.fontSize = 30;
                }
            }
            else if (CurrentFont == 3 && TextMesh.GetComponent<Renderer>().material != FontList[ThirdFont])
            {
                TextMesh.GetComponent<Renderer>().material = FontMList[ThirdFont];
                TextMesh.font = FontList[ThirdFont];
                TextMesh.color = Color.cyan;
                if (TextMesh.font == RockSalt)
                {
                    TextMesh.fontSize = 20;
                }
                else
                {
                    TextMesh.fontSize = 30;
                }
            }
        }
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Font Select #{0}]: {1}", FontSelect_moduleId, logData);
    }
}