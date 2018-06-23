using KMBombInfoExtensions;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MazeScrambler : MonoBehaviour
{
    private static int MazeScrambler_moduleIdCounter = 1;
    private int MazeScrambler_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable BRed;
    public KMSelectable BBlue;
    public KMSelectable BGreen;
    public KMSelectable BYellow;
    public KMSelectable Reset;
    protected MeshRenderer CurLED;
    public MeshRenderer[] LEDS;
    private int currentmaze = -1;

    protected bool SOLVED = false;
    protected int CurX;
    protected int CurY;
    protected int GoalX;
    protected int GoalY;
    protected int IDX1;
    protected int IDY1;
    protected int IDX2;
    protected int IDY2;
    protected int[] Xs;
    protected int[] Ys;
    protected int CurrentPress = 1;
    protected string[,] MazeWalls = new string[3, 3];
    protected int StartX;
    protected int StartY;
    protected string CurrentP;
    protected Color Orange = new Color(1, 0.5f, 0);

    public string TwitchHelpMessage = "Use !{0} NWSE, !{0} nwse, !{0} ULDR, or !{0} uldr to move North West South East.";

    protected KMSelectable[] ProcessTwitchCommand(string TPInput)
    {
        string tpinput = TPInput.ToLowerInvariant();
        bool Incomp = false;
        List<KMSelectable> Moves = new List<KMSelectable>();
        if (tpinput == "reset")
        {
            Moves.Add(Reset);
        }
        else if (tpinput == "red")
        {
            Moves.Add(BRed);
        }
        else if (tpinput == "blue")
        {
            Moves.Add(BBlue);
        }
        else if (tpinput == "yellow")
        {
            Moves.Add(BYellow);
        }
        else if (tpinput == "green")
        {
            Moves.Add(BGreen);
        }
        else
        {

            foreach (char c in tpinput)
            {
                if (c == 'r')
                {
                    Moves.Add(BRed);
                }
                else if (c == 'g')
                {
                    Moves.Add(BGreen);
                }
                else if (c == 'b')
                {
                    Moves.Add(BBlue);
                }
                else if (c == 'y')
                {
                    Moves.Add(BYellow);
                }
                else if (c == ' ')
                {
                }
                else
                {
                    Moves.Clear();
                    Incomp = true;
                }
            }
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



    private void Up()
    {
        if (CurrentP.Contains("U"))
        { 
            CurY--;
        }
        else
        {
            BombModule.HandleStrike();
            CurX = StartX;
            CurY = StartY;
            CurrentPress = 0;
        }
        UpdateLEDs();
    }

    private void Left()
    {
        if (CurrentP.Contains("L"))
        {
            CurX--;
        }
        else
        {
            BombModule.HandleStrike();
            CurX = StartX;
            CurY = StartY;
            CurrentPress = 0;
        }
        UpdateLEDs();
    }

    private void Right()
    {
        if (CurrentP.Contains("R"))
        {
            CurX++;
        }
        else
        {
            BombModule.HandleStrike();
            CurX = StartX;
            CurY = StartY;
            CurrentPress = 0;
        }
        UpdateLEDs();
    }

    private void Down()
    {
        if (CurrentP.Contains("D"))
        {
            CurY++;
        }
        else
        {
            BombModule.HandleStrike();
            CurX = StartX;
            CurY = StartY;
            CurrentPress = 0;
        }
        UpdateLEDs();
    }

    private void UpdateLEDs()
    {
        CurrentP = MazeWalls[CurY, CurX];
        if (CurX == GoalX && CurY == GoalY)
        {
            SOLVED = false;
            BombModule.HandlePass();
        }
        Xs = new int[9] { 0, 1, 2, 0, 1, 2, 0, 1, 2 };
        Ys = new int[9] { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
        for (int i = 0; i < 9; i++)
        {
            CurLED = LEDS[i];
            if (SOLVED)
            {
                if ((Xs[i] == IDX1 & Ys[i] == IDY1) ||(Xs[i] == IDX2 & Ys[i] == IDY2))
                {
                    if (Xs[i] == GoalX & Ys[i] == GoalY)
                    {
                        CurLED.material.color = Orange;
                    }
                    else if (Xs[i] == CurX & Ys[i] == CurY)
                    {
                        CurLED.material.color = Color.green;
                    }
                    else
                    {
                        CurLED.material.color = Color.yellow;
                    }
                }
                else
                {
                    if (Xs[i] == GoalX & Ys[i] == GoalY)
                    {
                        CurLED.material.color = Color.red;
                    }
                    else if (Xs[i] == CurX & Ys[i] == CurY)
                    {
                        CurLED.material.color = Color.blue;
                    }
                    else
                    {
                        CurLED.material.color = Color.black;
                    }
                }
            }
            else
            {
                CurLED.material.color = Color.black;
            }
        }
    }

    protected void Start()
    {
        MazeScrambler_moduleId = MazeScrambler_moduleIdCounter++;
        CurX = UnityEngine.Random.Range(0, 3);
        CurY = UnityEngine.Random.Range(0, 3);
        GoalX = UnityEngine.Random.Range(0, 3);
        GoalY = UnityEngine.Random.Range(0, 3);
        currentmaze = UnityEngine.Random.Range(1, 10);
        BombModule.OnActivate += OnActivate;
        StartX = CurX;
        StartY = CurY;
        if (GoalY == StartY)
        {
            GoalY++;
        }
        if (GoalY == 3)
        {
            GoalY = 0;
        }
        int[] IDXS = new int[18] { 1, 0, 1, 0, 1, 0, 2, 0, 1, 0, 2, 2, 2, 1, 2, 0, 0, 1 };
        int[] IDYS = new int[18] { 0, 0, 1, 1, 0, 1, 0, 0, 1, 2, 2, 2, 1, 2, 0, 2, 2, 2 };
        IDX1 = IDXS[currentmaze-1];
        IDY1 = IDYS[currentmaze-1];
        IDX2 = IDXS[currentmaze+8];
        IDY2 = IDYS[currentmaze+8];

        BRed.OnInteract += HandlePressRed;
        BBlue.OnInteract += HandlePressBlue;
        BGreen.OnInteract += HandlePressGreen;
        BYellow.OnInteract += HandlePressYellow;
        Reset.OnInteract += HandlePressReset;


        if (currentmaze == 1)
        {
            MazeWalls = new string[3, 3] {
                 { "D R", "L", "D"},
                 { "U R", "D R L", "U L"},
                 { "R", "U R L", "L"}
            };
        }
        if (currentmaze == 2)
        {
            MazeWalls = new string[3, 3] {
                 { "R", "D R L", "L"},
                 { "D R", "U D R L", "L"},
                 { "U", "U R", "L"}
            };
        }
        if (currentmaze == 3)
        {
            MazeWalls = new string[3, 3] {
                 { "D R", "R L", "L D"},
                 { "U", "D R", "U D L"},
                 { "R", "U L", "U"}
            };
        }
        if (currentmaze == 4)
        {
            MazeWalls = new string[3, 3] {
                 { "D R", "D R L", "L D"},
                 { "U D", "U D", "U"},
                 { "U", "U R", "L"}
            };
        }
        if (currentmaze == 5)
        {
            MazeWalls = new string[3, 3] {
                 { "R", "R L", "L D"},
                 { "D R", "R L", "U D L"},
                 { "U", "R", "U L"}
            };
        }
        if (currentmaze == 6)
        {
            MazeWalls = new string[3, 3] {
                 { "D", "D", "D"},
                 { "U D R", "U D R L", "U L"},
                 { "U", "U R", "L"}
            };
        }
        if (currentmaze == 7)
        {
            MazeWalls = new string[3, 3] {
                 { "D", "D", "D"},
                 { "U R", "U R L", "U D L"},
                 { "R", "R L", "U L"}
            };
        }
        if (currentmaze == 8)
        {
            MazeWalls = new string[3, 3] {
                 { "D R", "L", "D"},
                 { "U D R", "R L", "U D L"},
                 { "U", "R", "U L"}
            };
        }
        if (currentmaze == 9)
        {
            MazeWalls = new string[3, 3] {
                 { "D", "D R", "L"},
                 { "U D R", "U D R L", "L"},
                 { "U", "U R", "L"}
            };
        }

        CurrentP = MazeWalls[CurY, CurX];
        DebugLog("First Yellow LED is in [{0},{1}]", IDX1+1, IDY1+1);
        DebugLog("Second Yellow LED is in [{0},{1}]", IDX2+1, IDY2+1);
        DebugLog("Maze is {0} in reading order", currentmaze);
        DebugLog("Starting Position is [{0},{1}]", StartX + 1, StartY + 1);
        DebugLog("Goal location is [{0},{1}]", GoalX+1, GoalY+1);
        UpdateLEDs();
    }

    protected bool HandlePressRed()
    {
        //KMAudio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        BRed.AddInteractionPunch(0.5f);

        if (SOLVED)
        {
            if (CurrentPress % 10 == 1) 
            {
                Up();
            }
            else if (CurrentPress % 10 == 2)
            {
                Left();
            }
            else if (CurrentPress % 10 == 3)
            {
                Right();
            }
            else if (CurrentPress % 10 == 4)
            {
                Down();
            }
            else if (CurrentPress % 10 == 5)
            {
                Down();
            }
            else if (CurrentPress % 10 == 6)
            {
                Right();
            }
            else if (CurrentPress % 10 == 7)
            {
                Up();
            }
            else if (CurrentPress % 10 == 8)
            {
                Up();
            }
            else if (CurrentPress % 10 == 9)
            {
                Down();
            }
            else
            {
                Left();
            }

            CurrentPress++;
        }
        return false;
    }

    protected bool HandlePressBlue()
    {
        //KMAudio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        BBlue.AddInteractionPunch(0.5f);

        if (SOLVED)
        {
            if (CurrentPress % 10 == 1)
            {
                Left();
            }
            else if (CurrentPress % 10 == 2)
            {
                Right();
            }
            else if (CurrentPress % 10 == 3)
            {
                Left();
            }
            else if (CurrentPress % 10 == 4)
            {
                Up();
            }
            else if (CurrentPress % 10 == 5)
            {
                Right();
            }
            else if (CurrentPress % 10 == 6)
            {
                Up();
            }
            else if (CurrentPress % 10 == 7)
            {
                Left();
            }
            else if (CurrentPress % 10 == 8)
            {
                Right();
            }
            else if (CurrentPress % 10 == 9)
            {
                Up();
            }
            else
            {
                Down();
            }

            CurrentPress++;
        }

        return false;
    }

    protected bool HandlePressGreen()
    {
        //KMAudio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        BGreen.AddInteractionPunch(0.5f);

        if (SOLVED)
        {
            if (CurrentPress % 10 == 1)
            {
                Right();
            }
            else if (CurrentPress % 10 == 2)
            {
                Up();
            }
            else if (CurrentPress % 10 == 3)
            {
                Up();
            }
            else if (CurrentPress % 10 == 4)
            {
                Left();
            }
            else if (CurrentPress % 10 == 5)
            {
                Left();
            }
            else if (CurrentPress % 10 == 6)
            {
                Down();
            }
            else if (CurrentPress % 10 == 7)
            {
                Right();
            }
            else if (CurrentPress % 10 == 8)
            {
                Left();
            }
            else if (CurrentPress % 10 == 9)
            {
                Right();
            }
            else
            {
                Up();
            }

            CurrentPress++;
        }
        return false;
    }

    protected bool HandlePressYellow()
    {
        //KMAudio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        BYellow.AddInteractionPunch(0.5f);

        if (SOLVED)
        {
            if (CurrentPress % 10 == 1)
            {
                Down();
            }
            else if (CurrentPress % 10 == 2)
            {
                Down();
            }
            else if (CurrentPress % 10 == 3)
            {
                Right();
            }
            else if (CurrentPress % 10 == 4)
            {
                Right();
            }
            else if (CurrentPress % 10 == 5)
            {
                Up();
            }
            else if (CurrentPress % 10 == 6)
            {
                Left();
            }
            else if (CurrentPress % 10 == 7)
            {
                Down();
            }
            else if (CurrentPress % 10 == 8)
            {
                Down();
            }
            else if (CurrentPress % 10 == 9)
            {
                Left();
            }
            else
            {
                Right();
            }

            CurrentPress++;
        }
        return false;
    }

    protected bool HandlePressReset()
    {
        //KMAudio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Reset.AddInteractionPunch(0.5f);

        if (SOLVED)
        {
            CurX = StartX;
            CurY = StartY;
            CurrentPress = 1;
            UpdateLEDs();
        }

        return false;
    }

    protected void OnActivate()
    {
        SOLVED = true;
        UpdateLEDs();
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Maze Scrambler #{0}]: {1}", MazeScrambler_moduleId, logData);
    }
}