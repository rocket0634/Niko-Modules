using KMBombInfoExtensions;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

class ButtonInfo
{
	public int xOffset;
	public int yOffset;
	public string invalidDirection;
	public bool resetButton;

	public ButtonInfo(int x, int y, string dir, bool reset = false)
	{
		xOffset = x;
		yOffset = y;
		invalidDirection = dir;
		resetButton = reset;
	}
}

public class BlindMaze : MonoBehaviour
{
	private static int moduleIdCounter = 1;
	private int moduleId;
	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMAudio Audio;
    public KMRuleSeedable RuleSeed;
	public KMSelectable North;
	public KMSelectable East;
	public KMSelectable South;
	public KMSelectable West;
	public KMSelectable Reset;
	public MeshRenderer NorthMesh;
	public MeshRenderer EastMesh;
	public MeshRenderer SouthMesh;
	public MeshRenderer WestMesh;
    public static MonoRandom rng;
	int MazeRot, startRot;
	int currentMaze = -1;

	bool Solved = false;
	int MazeCode;
	int LastDigit;
	string CurrentP = "";
	int CurX;
	int CurY;
    int RotX;
    int RotY;
    string[,] MazeWalls = new string[5, 5];

	int StartX;
	int StartY;

    private List<string[,]> Mazes;

	private string TwitchHelpMessage = "Use !{0} nwse or !{0} uldr to move North West South East. Use !{0} reset to reset back to the start.";

	KMSelectable[] ProcessTwitchCommand(string input)
	{
		List<KMSelectable> Buttons = new List<KMSelectable>();

		string cleanInput = input.ToLowerInvariant();

        if (cleanInput.Equals("reset") || cleanInput.Equals("press reset"))
        {
            return new KMSelectable[1] { Reset };
        }

        if (cleanInput.StartsWith("move ") || cleanInput.StartsWith("press ") || cleanInput.StartsWith("walk ") || cleanInput.StartsWith("submit "))
        {
            cleanInput = cleanInput.Substring(cleanInput.IndexOf(" ", System.StringComparison.Ordinal) + 1);
        }
       
		foreach (char character in cleanInput)
		{
			switch (character)
			{
				case 'n':
				case 'u':
					Buttons.Add(North);
					break;
				case 'e':
				case 'r':
					Buttons.Add(East);
					break;
				case 's':
				case 'd':
					Buttons.Add(South);
					break;
				case 'w':
				case 'l':
					Buttons.Add(West);
					break;
                case ' ':
                    break;
				default:
					return null;
			}
		}

		return Buttons.ToArray();
	}

	int GetSolvedCount()
	{
		return BombInfo.GetSolvedModuleNames().Count;
	}

	void UpdatePosition(string Direction = "North", int xOffset = 0, int yOffset = 0, bool log = false)
	{
		CurX += xOffset;
		CurY += yOffset;
		
		if (CurX == 2 && CurY == -1) // They just moved out of the maze.
		{
			BombModule.HandlePass();
            StartCoroutine(ShowColor(curColor, prevColor));
            Solved = true;
			DebugLog("Moving {0}: The module has been disarmed.", Direction);
		}
		else
		{
            ButtonRotation(CurX, CurY);
			CurrentP = MazeWalls[CurY, CurX];
            if (log) DebugLog("Moving {0}: ({1}, {2})", Direction, RotX, RotY);
		}
	}

	int mod(int x, int m)
	{
		return (x % m + m) % m;
	}

	int[,] colorTable = new int[4, 5] {
		// Red Green White Gray Yellow
		{ 1, 5, 2, 2, 3 },	// North
		{ 3, 1, 5, 5, 2},	// East
		{ 3, 2, 4, 3, 2},	// South
        { 2, 5, 3, 1, 4}	// West
	};

	string[] buttonNames = { "North", "East", "South", "West" };
	string[] colorNames = { "red", "green", "blue", "gray", "yellow" };
	Color[] colors = { Color.red, Color.green, new Color(0, 100 / 255f, 1), Color.gray, Color.yellow }, prevColor;
    Color[] curColor
    {
        get
        {
            return new[] { NorthMesh.material.color, EastMesh.material.color, SouthMesh.material.color, WestMesh.material.color };
        }
    }
    int[] buttonColors = new int[4];

    void GenerateMazes() {
        if (rng.Seed == 1) {
            Mazes = new List<string[,]> {
                new string[5, 5] {
                    { "U L", "U", "N D R", "L U", "U R" },
                    { "L R", "D L ", "U", "D R", "L R" },
                    { "L D", "U R", "L D", "U R", "L D R" },
                    { "L U", "R", "L U R", "D L", "U R" },
                    { "D L R", "L D", "D R", "D U L", "R D" }
                },
                new string[5, 5] {
                    { "U D L", "U R", "N R L", "U L", "U R" },
                    { "U L", "D", "R D", "R L", "R D L" },
                    { "L", "U", "U D", "D", "U R" },
                    { "R L", "R D L", "U R L", "U D L", "R" },
                    { "D L", "U D", "R D", "U D L", "R D" }
                },
                new string[5, 5] {
                    { "U L", "U R D", "N L", "U R D", "L U R" },
                    { "L D", "U D", "D", "U D", "R" },
                    { "L U D", "U", "U", "U", "D R" },
                    { "L U R", "R L", "R L", "L D", "U R" },
                    { "D L", "D R", "L D", "U D R", "L R D" }
                },
                new string[5, 5] {
                    { "U L D", "U R", "L N D", "U", "U R" },
                    { "L U", "D", "D U", "D R", "L R" },
                    { "L", "U R", "U L", "U", "R D" },
                    { "L D R", "L R D", "R L", "L", "U R" },
                    { "D L U", "D U", " D R", "L D R", " L D R" }
                },
                new string[5, 5] {
                    { "U L",   "U",     "N D",   "U R", "L U R" },
                    { "L D R", "L R",   "L U R", "L D", "R" },
                    { "L U",   "D",     "D R",   "L U", "D R" },
                    { "L R",   "U D L", "R U",   "D L", "R U" },
                    { "D L",   "U D R",     "D L",     "U D", "R D" }
                },
                new string[5, 5] {
                    { "U L",   "U R", "L D N", "U D",   "U R"   },
                    { "L R",   "L",   "U R D", "U L",   "D R"   },
                    { "L R",   "L",   "U D",   "",      "U R"   },
                    { "L R D", "L R", "U L",   "R D",   "L R"   },
                    { "D L U", "R D", "D L",   "D U R", "L R D" }
                },
                new string[5, 5] {
                    { "U L", "U", "N D", "U D", "U R"},
                    { "L R", "L D R", "L U", "U R", "L R"},
                    { "L R", "U D L", "D R", "L", "R"},
                    { "L D", "U R", "U D L", "R D", "L R"},
                    { "D L U", "D", "D R U", "D L U", "R D"}
                },
                new string[5, 5] {
                    { "U L D", "U R",   "R N L", "U L R", "L U R" },
                    { "L U",   "R",     "R L",   "L",     "D R" },
                    { "L R",   "L D",   "R",     "L D",   "U R" },
                    { "L R",   "L R U", "L D",   "U R",   "L R" },
                    { "D L R", "L D",   "U D",   "D",     "R D" }
                },
                new string[5, 5] {
                    { "U L",   "U D",   "N R",   "U L",   "U R D" },
                    { "L R",   "U L",   "D",     "D",     "U R" },
                    { "L R",   "L D",   "U R D", "U L D", "R" },
                    { "L D",   "U R D", "U L",   "U R",   "L R" },
                    { "D L U", "U D",   "D R",   "L D",   "R D" }
                },
                new string[5, 5] {
                    { "U L R", "L U D", "N R", "L U", "U R"   },
                    { "L D",   "U R",   "L R", "L R", "L R D" },
                    { "L U R", "D L",   "D",   "",    "D R"     },
                    { "L",     "U R",   "U L", "",    "U D R"     },
                    { "D L R", "D L",   "D R", "D",   "U R D"   }
                }
            };
        } else {
            Mazes = new List<string[,]>();
            MazeGeneration.InitializeGeneration();
            var cardinal = new[] { "N", "E", "W", "S" };
            var direction = new[] { "U", "R", "L", "D" };

            for (var i = 0; i < 10; i++)
                Mazes.Add(Enumerable.Range(0, 5).SelectMany(r => Enumerable.Range(0, 5).Select(c => Enumerable.Range(0, 4).Select(d => (!MazeGeneration.Cells[i, r, c][cardinal[d]]) ? direction[d] : "").Join(" "))).ToArray().ToArray2D(5, 5));
        }
    }


	void Start()
	{
		moduleId = moduleIdCounter++;

        rng = RuleSeed.GetRNG();
        DebugLog("Using rule seed: {0}", rng.Seed);
        GenerateMazes();

		//check what the serial ends with and make an integer for it
		LastDigit = BombInfo.GetSerialNumberNumbers().Last();

        if (rng.Seed != 1) {
            for (var i = 0; i < 20; i++) {
                colorTable[i / 5, i % 5] = rng.Next(1, 5);
            }
        }

		int SumNS = 4;
		int SumEW = 4;
        int[] COLORKEYS = new int[5];

		for (int i = 0; i < 4; i++)
		{
			buttonColors[i] = UnityEngine.Random.Range(0, 5);
			var colorNum = buttonColors[i];

			int value = colorTable[i, colorNum];
			DebugLog("{0} Key is {1}, making it's value {2}", buttonNames[i], colorNames[colorNum], value);

            COLORKEYS[colorNum]++; // Adds the current color

			if (i % 2 == 0) SumNS += value; // North and South are both on even indexes
			else SumEW += value; // East and West are both on odd indexes
		}
		
		SumNS = SumNS % 5;
		SumEW = SumEW % 5;

        // Look for mazebased modules
        string[] MazeModules = new[] { "Mouse In The Maze", "3D Maze", "Hexamaze", "Morse-A-Maze", /*"Blind Maze",*/ "Polyhedral Maze", "Maze", "USA Maze", "Maze Scrambler", "Boolean Maze", "The Crystal Maze", "Factory Maze", "Module Maze" };
        int MazeBased = BombInfo.GetModuleNames().Intersect(MazeModules).Count();
        DebugLog("There are {0} compatible maze-type modules on the bomb, not including Blind Maze.", MazeBased);

        int MazeRule = 7;
        int[] rulesCount = { COLORKEYS[0], BombInfo.GetBatteryCount(), COLORKEYS[4], COLORKEYS[0], MazeBased, BombInfo.GetPorts().Distinct().Count(), COLORKEYS[1], COLORKEYS[2], COLORKEYS[3], BombInfo.GetBatteryCount(KMBI.KnownBatteryType.AA), BombInfo.GetBatteryCount(KMBI.KnownBatteryType.D), BombInfo.GetBatteryHolderCount(), BombInfo.GetPortCount(), BombInfo.GetPortPlateCount(), BombInfo.GetIndicators().Count(), BombInfo.GetOnIndicators().Count(), BombInfo.GetOffIndicators().Count() };
        int[] conditionCount = { 2, 5, 1, 1, 1 };
        int conditionIndicator = 3;
        int[] mazeRotation = { 1, 1, 2, 1, 2, 1 };
        bool[] clockwise = { true, true, true, false, true, false };
        bool[] calculate = { false, true, false, false, true, true };

        Action<int> rules = (x) => {
            if (clockwise[x]) startRot = MazeRot = mazeRotation[x];
            else startRot = MazeRot = 4 - mazeRotation[x];

            if (calculate[x]) startRot = 0;

            MazeRule = x + 1;
        };

        if (rng.Seed != 1) {
            rulesCount = rng.ShuffleFisherYates(rulesCount).Take(6).ToArray();
            conditionIndicator = rng.Next(11);

            for (var i = 0; i < 6; i++) {
                if (i < 5) conditionCount[i] = rng.Next(1, 5);

                mazeRotation[i] = rng.Next(1, 4);
                clockwise[i] = (rng.Next(2) == 1) ? true : false;
                calculate[i] = (rng.Next(2) == 1) ? true : false;
            }
        }

        //Determine rotation
        if (rulesCount[0] >= conditionCount[0]) {
            rules(0);
        } else if (rulesCount[1] >= conditionCount[1]) {
            rules(1);
        } else if (BombInfo.GetIndicators().Contains(((KMBI.KnownIndicatorLabel)conditionIndicator).ToString())) {
            rules(2);
        } else if (rulesCount[2] == 0 && rulesCount[3] >= conditionCount[2]) {
            rules(3);
        } else if (rulesCount[4] >= conditionCount[3]) {
            rules(4);
        } else if (rulesCount[5] <= conditionCount[4]) {
            rules(5);
        } else {
            startRot = MazeRot = 0;
            MazeRule = 7;
        }

        var ruleClockwise = "clockwise";

        if (MazeRule != 7) ruleClockwise = (clockwise[MazeRule - 1]) ? "clockwise" : "counter-clockwise";

        DebugLog("Maze Rotation is {0} degrees {1} because of rule {2}", MazeRule == 7 ? 0 : mazeRotation[MazeRule - 1] * 90, ruleClockwise, MazeRule);

		KMSelectable[] directions = new[] { North, East, South, West };
		ButtonInfo[] buttonInfo = new[]
		{
			new ButtonInfo(0, -1, "U"),
			new ButtonInfo(1, 0, "R"),
			new ButtonInfo(0, 1, "D"),
			new ButtonInfo(-1, 0, "L")
		};

		for (int i = 0; i < 4; i++)
		{
			directions[i].OnInteract += GetInteractHandler(directions[i], buttonInfo[mod(-MazeRot + i, 4)]);
		}

		Reset.OnInteract += GetInteractHandler(Reset, new ButtonInfo(0, 0, null, true));

        //Determine Starting Position
		switch (startRot)
		{
			case 0:
				CurX = SumNS;
				CurY = SumEW;
				break;
			case 1:
				CurX = SumEW;
				CurY = 4 - SumNS;
				break;
			case 2:
				CurX = 4 - SumNS;
				CurY = 4 - SumEW;
                break;
			case 3:
				CurX = 4 - SumEW;
				CurY = SumNS;
                break;
		}
		UpdatePosition();

		StartX = CurX;
		StartY = CurY;

        DebugLog("Starting location is: ({0}, {1}).", SumNS + 1, SumEW + 1);

        if (MazeRot != startRot) DebugLog("Location is determined before rotation. Starting location on rotated maze is ({0}, {1}).", RotX, RotY);
        else if (MazeRule != 7) DebugLog("Location is determined after rotation.");
        prevColor = curColor.ToArray();
        var nextColor = buttonColors.Select(x => colors[x]).ToArray();
        BombModule.OnActivate += delegate () { StartCoroutine(ShowColor(prevColor, nextColor)); };
    }

    IEnumerator ShowColor(Color[] start, Color[] end)
    {
        MeshRenderer[] buttonRenderers = { NorthMesh, EastMesh, SouthMesh, WestMesh };
        var t = 0f;
        while (buttonRenderers.All(x => x.material.color != end[Array.IndexOf(buttonRenderers, x)]))
        {
            for (int i = 0; i < 4; i++)
            {
                buttonRenderers[i].material.color = Color.Lerp(start[i], end[i], t);
            }
            t += 0.015f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    void ButtonRotation(int x, int y)
    {
        switch (MazeRot)
        {
            case 0:
                RotX = x + 1;
                RotY = y + 1;
                break;
            case 1:
                RotX = 5 - y;
                RotY = x + 1;
                break;
            case 2:
                RotX = 5 - x;
                RotY = 5 - y;
                break;
            case 3:
                RotX = y + 1;
                RotY = 5 - x;
                break;
        }
    }

	KMSelectable.OnInteractHandler GetInteractHandler(KMSelectable selectable, ButtonInfo buttonInfo)
	{
		return delegate ()
		{
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			selectable.AddInteractionPunch(0.5f);
            string Direction = selectable.ToString().Split(' ').First();

			if (!Solved)
			{
                if (buttonInfo.resetButton)
                {
                    CurX = StartX;
                    CurY = StartY;
                    UpdatePosition();
                    ButtonRotation(CurX, CurY);
                    DebugLog("Resetted, now at ({0}, {1})", RotX, RotY);
                }
                else if (CurrentP.Contains(buttonInfo.invalidDirection))
                {
                    ButtonRotation(CurX, CurY);
                    DebugLog("There is a wall to the {0} at ({1}, {2}). Strike.", Direction, RotX, RotY);
                    BombModule.HandleStrike();
                }
                else
                    UpdatePosition(Direction, buttonInfo.xOffset, buttonInfo.yOffset, true);
			}

			return false;
		};
	}

    private void Update()
    {
        int MazeNumber = (LastDigit + GetSolvedCount()) % 10;
        if (currentMaze != GetSolvedCount() && !Solved)
        {
            currentMaze = GetSolvedCount();
            DebugLog("The Maze Number is now {0}", MazeNumber);
			MazeWalls = Mazes[MazeNumber];
			UpdatePosition();
        }
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Blind Maze #{0}] {1}", moduleId, logData);
    }
}