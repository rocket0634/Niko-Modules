using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class FontSelect : MonoBehaviour
{
    private FontSettings Settings = new FontSettings();
    private bool banned = false;
    private static int FontSelect_moduleIdCounter = 1;
    private int FontSelect_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Left;
    public KMSelectable Right;
    public KMSelectable Submit;
    public TextMesh TextMesh;
    public Material[] TextMaterials;

    public Font[] Fonts;

    protected bool Solved;
    protected int PhraseNumber;
    protected int FirstFont, f;
    protected int SecondFont, s;
    protected int ThirdFont, t;
    protected int CurrentFont = 1;
    protected bool FontSelected;
    private int[] FontList = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

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
        ModConfig modConfig = new ModConfig("FontSettings", typeof(FontSettings));
        Settings = (FontSettings)modConfig.Settings;
        modConfig.Settings = Settings;
        FontSelect_moduleId = FontSelect_moduleIdCounter++;
        banned = Settings.disableKarmaMerriweather;
        int[] ListNumbers = { 1, 4, 4, 2, 3, 3, 1, 2, 3, 1, 3, 4, 4, 1, 2, 2 };
        string[] ListPhrases = { "Eight Ate 8", "Jokes on you!\nI'm the male.", "Jokes on you!\nI'm male.", "Testing,\ntesting,\n1 to 3.", "Yew R. Wonn", "Jokes on you!\nI'm the mail.", "Ewe Arr Won", "888", "U.R. 1", "You are one", "Ate, Ate, Ate", "8 ate eight", "Testing,\ntesting,\n123", "Testing,\ntesting,\n1-3", "Jokes on you!\nI'm female.", "Testing,\ntesting,\n1 two 3." };

        PhraseNumber = UnityEngine.Random.Range(0, ListPhrases.Count());

        int FontListNum = ListNumbers[PhraseNumber];
        string Phrase = ListPhrases[PhraseNumber];
        switch (FontListNum)
        {
            case 1:
                FontList = new int[] { 1, 6, 8, 4, 2, 10, 5, 9, 3, 0, 7, 11 };
                FontSelected = true;
                break;
            case 2:
                FontList = new int[] { 0, 3, 4, 8, 11, 2, 10, 7, 5, 6, 1, 9 };
                FontSelected = true;
                break;
            case 3:
                FontList = new int[] { 8, 6, 10, 3, 0, 1, 9, 5, 11, 4, 7, 2 };
                FontSelected = true;
                break;
            case 4:
                FontList = new int[] { 10, 7, 5, 1, 11, 2, 9, 8, 4, 0, 6, 3 };
                FontSelected = true;
                break;
        }

        FirstFont = UnityEngine.Random.Range(0, Fonts.Count());
        SecondFont = UnityEngine.Random.Range(0, Fonts.Count());
        ThirdFont = UnityEngine.Random.Range(0, Fonts.Count());
        
        while (banned && (Fonts[FontList[FirstFont]].name == "Karma-Regular" || Fonts[FontList[FirstFont]].name == "Merriweather-Light")) FirstFont = UnityEngine.Random.Range(0, Fonts.Count());
        while (SecondFont == FirstFont || (banned && (Fonts[FontList[SecondFont]].name == "Karma-Regular" || Fonts[FontList[SecondFont]].name == "Merriweather-Light")))
        {
            SecondFont = UnityEngine.Random.Range(0, Fonts.Count());
        }
        while (ThirdFont == FirstFont || ThirdFont == SecondFont || (banned && (Fonts[FontList[ThirdFont]].name == "Karma-Regular" || Fonts[FontList[ThirdFont]].name == "Merriweather-Light")))
        {
            ThirdFont = UnityEngine.Random.Range(0, Fonts.Count());
        }

        TextMesh.text = Phrase;
        f = FontList[FirstFont];
        s = FontList[SecondFont];
        t = FontList[ThirdFont];

        Left.OnInteract = HandlePressL;
        Right.OnInteract = HandlePressR;
        Submit.OnInteract = HandlePressSubmit;

        DebugLog("Phrase is \"{1}\" which makes the List Number {0}", FontListNum, Phrase.Replace("\n", " "));
        DebugLog("First Font is \"{0}\", Second Font is \"{1}\", Third Font is \"{2}\"", Fonts[f].name, Fonts[s].name, Fonts[t].name);
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
        WriteToScreen();
    }

    protected bool HandlePressL()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Left.AddInteractionPunch(0.5f);

        if (!Solved)
        {
            if (CurrentFont != 1)
            {
                CurrentFont = CurrentFont - 1;
                WriteToScreen();
            }
        }
        return false;
    }

    protected bool HandlePressR()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Right.AddInteractionPunch(0.5f);

        if (!Solved)
        {
            if (CurrentFont != 3)
            {
                CurrentFont = CurrentFont + 1;
                WriteToScreen();
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

    private void WriteToScreen()
    {
        var a = new[] { f, s, t };
        var b = a[CurrentFont - 1];
        if (FontSelected == true)
        {
            TextMesh.GetComponent<Renderer>().material = TextMaterials[b];
            TextMesh.font = Fonts[b];
            TextMesh.color = Color.cyan;
            if (TextMesh.font.name == "RockSalt-Regular")
            {
                TextMesh.fontSize = 80;
            }
            else if (TextMesh.font.name == "DayPosterBlack")
            {
                TextMesh.fontSize = 118;
            }
            else
            {
                TextMesh.fontSize = 120;
            }
        }
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "FontSettings.json" },
            { "Name", "Font Select" },
            { "Listing", new List<Dictionary<string,object>> {
                new Dictionary<string, object>
                {
                    { "Key", "disableKarmaMerriweather" },
                    { "Text", "Disable Karma and Merriweather" },
                }
            } }
        }
    };

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Font Select #{0}] {1}", FontSelect_moduleId, logData);
    }
}
class FontSettings
{
    public bool disableKarmaMerriweather = false;
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
            return Path.Combine(Application.persistentDataPath, "Modsettings\\" + _filename + ".json");
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