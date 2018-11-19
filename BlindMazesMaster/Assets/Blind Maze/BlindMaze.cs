using KMBombInfoExtensions;
using UnityEngine;
using System.Linq;
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
    private MonoRandom rng;
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

	private List<string[,]> Mazes = new List<string[,]> {
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
			Solved = true;
			DebugLog("Moving {0}: The module has been defused.", Direction);
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
	string[] colorNames = { "red", "green", "white", "gray", "yellow" };
	Color[] colors = { Color.red, Color.green, Color.white, Color.gray, Color.yellow };

	void Start()
	{
        rng = RuleSeed.GetRNG();

		moduleId = moduleIdCounter++;

		//check what the serial ends with and make an integer for it
		LastDigit = BombInfo.GetSerialNumberNumbers().Last();

		int SumNS = 4;
		int SumEW = 4;

		int REDKEY = 0;
		bool NOYELLOW = true;

		MeshRenderer[] buttonRenderers = { NorthMesh, EastMesh, SouthMesh, WestMesh };
		for (int i = 0; i < 4; i++)
		{
			int colorNum = UnityEngine.Random.Range(0, 5);
			buttonRenderers[i].material.color = colors[colorNum];

			int value = colorTable[i, colorNum];
			DebugLog("{0} Key is {1}, making it's value {2}", buttonNames[i], colorNames[colorNum], value);

			if (colorNum == 0) REDKEY++; // Red is index 0.
			else if (colorNum == 4) NOYELLOW = false; // Yellow is index 4.

			if (i % 2 == 0) SumNS += value; // North and South are both on even indexes
			else SumEW += value; // East and West are both on odd indexes
		}
		
		SumNS = SumNS % 5;
		SumEW = SumEW % 5;

        // Look for mazebased modules
		string[] MazeModules = new[] { "Mouse In The Maze", "3D Maze", "Hexamaze", "Morse-A-Maze", "Blind Maze", "Polyhedral Maze", "Maze", "USA Maze", "Maze Scrambler", "Boolean Maze", "The Crystal Maze" };
		int MazeBased = BombInfo.GetModuleNames().Intersect(MazeModules).Count();
        DebugLog("There are {0} compatible maze-type modules on the bomb, including Blind Maze.", MazeBased);

		// Determine rotation
		int MazeRule;
		if (REDKEY > 1)
		{
			MazeRot = 1;
            startRot = 1;
			MazeRule = 1;
		}
		else if (BombInfo.GetBatteryCount() > 4)
		{
			MazeRot = 1;
            startRot = 0;
			MazeRule = 2;
		}
		else if (BombInfo.GetIndicators().Contains("IND"))
		{
			MazeRot = 2;
            startRot = 2;
			MazeRule = 3;
		}
		else if (REDKEY > 0 && NOYELLOW == true)
		{
			MazeRot = 3;
            startRot = 3;
			MazeRule = 4;
		}
		else if (MazeBased > 1)
		{
			MazeRot = 2;
            startRot = 0;
			MazeRule = 5;
		}
		else if (BombInfo.GetPorts().Distinct().Count() < 2)
		{
			MazeRot = 3;
            startRot = 0;
			MazeRule = 6;
		}
		else
		{
			MazeRot = 0;
            startRot = 0;
			MazeRule = 7;
		}

		DebugLog("Maze Rotation is {0} degrees clockwise because of rule {1}", MazeRot * 90, MazeRule);

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
                    DebugLog(rng.Next(0, 6).ToString());
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
