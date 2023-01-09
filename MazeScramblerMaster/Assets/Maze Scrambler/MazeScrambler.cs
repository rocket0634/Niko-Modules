using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MazeScrambler : MonoBehaviour
{
    private static int MazeScrambler_moduleIdCounter = 1;
    private int MazeScrambler_moduleId;
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

    protected bool SOLVED,strike = false;
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
    protected string[] Red = new string[] { "Up", "Left", "Right", "Down", "Down", "Right", "Up", "Up", "Down", "Left", "Red" },
        Blue = new string[] { "Left", "Right", "Left", "Up", "Right", "Up", "Left", "Right", "Up", "Down", "Blue" },
        Green = new string[] { "Right", "Up", "Up", "Left", "Left", "Down", "Right", "Left", "Right", "Up", "Green" },
        Yellow = new string[] { "Down", "Down", "Right", "Right", "Up", "Left", "Down", "Down", "Left", "Right", "Yellow" };

    private string TwitchHelpMessage = "Use !{0} rgby to press the Red, Green, Blue, and Yellow buttons. Use !{0} Reset to reset the module.";
    

    protected KMSelectable[] ProcessTwitchCommand(string TPInput)
    {
        string tpinput = TPInput.ToLowerInvariant();
        var command = tpinput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (tpinput.Equals("reset")) return new KMSelectable[] { Reset };
        List<KMSelectable> Moves = new List<KMSelectable>();
        foreach (string s in command)
        {
            switch (s)
            {
                case "press":
                case " ":
                    break;
                case "red":
                    Moves.Add(BRed);
                    break;
                case "blue":
                    Moves.Add(BBlue);
                    break;
                case "green":
                    Moves.Add(BGreen);
                    break;
                case "yellow":
                    Moves.Add(BYellow);
                    break;
                default:
                    foreach (char c in s)
                    {
                        switch (c)
                        {
                            case 'r':
                                Moves.Add(BRed);
                                break;
                            case 'b':
                                Moves.Add(BBlue);
                                break;
                            case 'g':
                                Moves.Add(BGreen);
                                break;
                            case 'y':
                                Moves.Add(BYellow);
                                break;
                            default:
                                return null;
                        }
                    }
                    break;
            }
        }
        return Moves.ToArray();
    }

    private bool Direction(string dir)
    {
        if (CurrentP.Contains(dir.First().ToString()))
        {
            switch (dir)
            {
                case "Up":
                    CurY--;
                    break;
                case "Left":
                    CurX--;
                    break;
                case "Right":
                    CurX++;
                    break;
                case "Down":
                    CurY++;
                    break;
            }
        }
        else
        {
            DebugLog("--------------------------------------------------");
            DebugLog("Wall detected trying to move {0} from [{1},{2}]", dir.ToLowerInvariant(), CurX + 1, CurY + 1);
            strike = true;
            BombModule.HandleStrike();
            CurX = StartX;
            CurY = StartY;
            CurrentPress = 1;
            DebugLog("Sequence reset:\n[Maze Scrambler #{0}]: Sel | New Pos | Red | Blue | Yellow | Green", MazeScrambler_moduleId);
        }
        UpdateLEDs(dir);
        return strike;
    }

    private void UpdateLEDs(string dir = null)
    {
        CurrentP = MazeWalls[CurY, CurX];
        if (CurX == GoalX && CurY == GoalY)
        {
            DebugLog("--------------------------------------------------");
            DebugLog("Moved {0} to [{1},{2}]. Module solved.", dir.ToLowerInvariant(), GoalX + 1, GoalY + 1);
            SOLVED = true;
            BombModule.HandlePass();
        }
        Xs = new int[9] { 0, 1, 2, 0, 1, 2, 0, 1, 2 };
        Ys = new int[9] { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
        for (int i = 0; i < 9; i++)
        {
            CurLED = LEDS[i];
            if (!SOLVED)
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
        int[] IDXS = new int[18] { 1, 0, 1, 0, 1, 0, 2, 0, 1, 0, 2, 2, 2, 1, 2, 0, 0, 1 };
        int[] IDYS = new int[18] { 0, 0, 1, 1, 0, 1, 0, 0, 1, 2, 2, 2, 1, 2, 0, 2, 2, 2 };
        IDX1 = IDXS[currentmaze-1];
        IDY1 = IDYS[currentmaze-1];
        IDX2 = IDXS[currentmaze+8];
        IDY2 = IDYS[currentmaze+8];

        BRed.OnInteract += delegate () { HandlePress(BRed, Red); return false; };
        BBlue.OnInteract += delegate () { HandlePress(BBlue, Blue); return false; };
        BGreen.OnInteract += delegate () { HandlePress(BGreen, Green); return false; };
        BYellow.OnInteract += delegate () { HandlePress(BYellow, Yellow); return false; };
        Reset.OnInteract += delegate () { HandlePress(Reset); return false; };


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

        var blacklist = new List<int[]>();
        blacklist.Add(new[] { CurY, CurX });
        blacklist.AddRange(PathChecker(CurY, CurX, blacklist));
        var newBlacklist = new List<int[]>(blacklist.ToList());
        var count = new List<int>();
        foreach (var item in blacklist)
        {
            newBlacklist.Concat(PathChecker(item[0], item[1], newBlacklist));
        }
        foreach (var item in newBlacklist)
        {
            if (blacklist.Any(arg => arg.SequenceEqual(item)))
            {
                continue;
            }
            count.Add(PathChecker(item[0], item[1], newBlacklist.ToList()).Count());
        }
        if (!count.Any(arg => arg > 2)) blacklist = new List<int[]>(newBlacklist);
        while (blacklist.Any(arg => arg.SequenceEqual(new[] { GoalY, GoalX })))
        {
            GoalX = UnityEngine.Random.Range(0, 3);
            GoalY = UnityEngine.Random.Range(0, 3);
        }

        CurrentP = MazeWalls[CurY, CurX];
        DebugLog("First Yellow LED is in [{0},{1}]", IDX1+1, IDY1+1);
        DebugLog("Second Yellow LED is in [{0},{1}]", IDX2+1, IDY2+1);
        DebugLog("Maze is {0} in reading order", currentmaze);
        DebugLog("Starting Position is [{0},{1}]", StartX + 1, StartY + 1);
        DebugLog("Goal location is [{0},{1}]", GoalX+1, GoalY+1);
        DebugLog("Current sequence is:\n[Maze Scrambler #{0}]: Sel | New Pos | Red | Blue | Yellow | Green", MazeScrambler_moduleId);
        DebugLog("----- | [-,-] | {0} | {1} | {2} | {3}", Red[0], Blue[0], Yellow[0], Green[0]);
        UpdateLEDs();
    }

    List<int[]> PathChecker(int y, int x, List<int[]> blacklist)
    {
        var letters = new[] { 'U', 'D', 'L', 'R' };
        var path = new[] { new[]{ y - 1, x}, new[]{ y + 1, x }, new[]{ y, x - 1 }, new[]{ y, x + 1 } };
        foreach (char c in MazeWalls[y, x])
        {
            if (c == ' ') continue;
            var letter = Array.IndexOf(letters, c);
            if (!blacklist.Any(arg => arg.SequenceEqual(path[letter]))) blacklist.Add(path[letter]);
        }
        return blacklist;
    }

    protected bool HandlePress(KMSelectable B, string[] color = null)
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        B.AddInteractionPunch(0.5f);

        if (!SOLVED)
        {
            if (B == Reset)
            {
                CurX = StartX;
                CurY = StartY;
                CurrentPress = 1;
                UpdateLEDs();
                DebugLog("--------------------------------------------------");
                DebugLog("Sequence reset:\n[Maze Scrambler #{0}]: Sel | New Pos | Red | Blue | Yellow | Green", MazeScrambler_moduleId);
                DebugLog("Reset | [{4},{5}] | {0} | {1} | {2} | {3}", Red[0], Blue[0], Yellow[0], Green[0], CurX + 1, CurY + 1);

                return false;
            }
            if (!Direction(color[(CurrentPress - 1) % 10]))
                CurrentPress++;
            else
                strike = !strike;
            var c = (CurrentPress - 1) % 10;
            if (!SOLVED) DebugLog("{0} | [{1},{2}] | {3} | {4} | {5} | {6}", color.Last(), CurX + 1, CurY + 1, Red[c], Blue[c], Yellow[c], Green[c]);
        }
        return false;
    }

    protected void OnActivate()
    {
        UpdateLEDs();
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Maze Scrambler #{0}] {1}", MazeScrambler_moduleId, logData);
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        var end = false;
        var movement = new Stack<int[]>();
        var explored = new List<int[]>();
        movement.Push(new[] { GoalX, GoalY, -1 });
        var direction = new[] { 0, CurrentPress - 1 };
        var dir = new[] { 'D', 'L', 'U', 'R' };
        var curLoc = new[] { GoalX, GoalY };
        var color = new[] { Red, Blue, Green, Yellow };
        var moveDir = new Func<int[]>[] { () => new[] { curLoc[0], curLoc[1] + 1 }, () => new[] { curLoc[0] - 1, curLoc[1] }, () => new[] { curLoc[0], curLoc[1] - 1 }, () => new[] { curLoc[0] + 1, curLoc[1] } };
        while (!end)
        {
            if (direction[0] > 3)
            {
                movement.Pop();
                explored.Add(curLoc.ToArray());
                curLoc = moveDir[(movement.Peek()[2] + 2) % 4]();
                direction[0] = movement.Peek()[2] + 1;
                continue;
            }
            if (MazeWalls[curLoc[1], curLoc[0]].Contains(dir[direction[0]]))
            {
                curLoc = moveDir[direction[0]]();
                if (movement.Any(x => x[0] == curLoc[0] && x[1] == curLoc[1]) || explored.Any(x => x.SequenceEqual(curLoc)))
                    curLoc = moveDir[(direction[0] + 2) % 4]();
                else
                {
                    movement.Peek()[2] = direction[0];
                    movement.Push(new[] { curLoc[0], curLoc[1], 0 });
                    direction[0] = -1;
                }
            }
            direction[0] += 1;
            if (curLoc.SequenceEqual(new[] { CurX, CurY }))
                end = true;
            yield return null;
        }
        movement.Pop();
        end = !end;
        var buttons = new List<int>();
        foreach (int step in movement.Select(x => x[2]))
        {
            int curStep = (step + 2) % 4;
            while (!color.Any(x => x[direction[1] % 10].Contains(dir[curStep])))
            {
                var validChars = MazeWalls[curLoc[1], curLoc[0]].Replace(" ", "").Replace(dir[curStep].ToString(), "");
                for (int i = 0; i < validChars.Count(); i++)
                {
                    var button = color.First(x => x[direction[1] % 10].Contains(validChars[i]));
                    var charDir = Array.IndexOf(dir, validChars[i]);
                    var inverseCharDir = charDir + 2;
                    if (button == null)
                    {
                        DebugLog("Something went wrong");
                        BombModule.HandlePass();
                        yield break;
                    }
                    var index = Array.IndexOf(color, button);
                    curLoc = moveDir[charDir]();
                    if (color.Any(x => x[(direction[1] + 1) % 10].Contains(dir[inverseCharDir % 4])))
                    {
                        buttons.Add(index);
                        DebugLog("Adding button {0} with expected value of {1}", index, validChars[i]);
                        button = color.First(x => x[(direction[1] + 1) % 10].Contains(dir[inverseCharDir % 4]));
                        if (button == null)
                        {
                            DebugLog("Something else went wrong");
                            BombModule.HandlePass();
                            yield break;
                        }
                        index = Array.IndexOf(color, button);
                        buttons.Add(index);
                        DebugLog("Adding button {0} with expected value of {1}", index, dir[inverseCharDir % 4]);
                        direction[1] += 2;
                        curLoc = moveDir[inverseCharDir % 4]();
                        break;
                    }
                    curLoc = moveDir[inverseCharDir % 4]();
                }
                yield return null;
            }
            var selectButton = color.First(x => x[direction[1] % 10].Contains(dir[curStep]));
            if (selectButton == null)
            {
                DebugLog("Who's on first? Not me, I'm broke.");
                BombModule.HandlePass();
                yield break;
            }
            buttons.Add(Array.IndexOf(color, selectButton));
            direction[1] += 1;
            curLoc = moveDir[curStep]();
        }
        DebugLog(string.Join(", ", buttons.Select(x => x.ToString()).ToArray()));
        if (!curLoc.SequenceEqual(new int[] { GoalX, GoalY }))
        {
            DebugLog("Whoops. {0}, {1}, {2}, {3}", curLoc[0], curLoc[1], GoalX, GoalY);
            BombModule.HandlePass();
            yield break;
        }
        var selectables = new[] { BRed, BBlue, BGreen, BYellow };
        foreach (int num in buttons)
        {
            selectables[num].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}