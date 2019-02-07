using KMBombInfoExtensions;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class Switch : MonoBehaviour
{
    private static int Switch_moduleIdCounter = 1;
    private int Switch_moduleId;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable FlipperSelectable;
    public Transform FlipperPosition;
    public MeshRenderer TopLED;
    public MeshRenderer BottomLED;

    protected bool SOLVED = false;
    protected bool FlipperDown = false;
    protected bool FirstSuccess = false;
    protected bool InitMove = true;
    protected bool FlipperMoving = false;
    protected int BottomColor;
    protected int TopColor;
    protected int TimerSeconds1;
    protected int TimerSeconds2;
    protected int NeededNumber;
    
    public string TwitchHelpMessage = "Use !{0} flip 5 to flip when the seconds digits of the timer contains 5";

    System.Collections.IEnumerator ProcessTwitchCommand(string command)
    {
        string LowerCommand = command.ToLower();
        if (Regex.IsMatch(LowerCommand, @"\s*(flip )[0-9]\s*$"))
        {
            var split = LowerCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int PressOn = 0;
            if (int.TryParse(split[1], out PressOn))
            {
                while (TimerSeconds1 != PressOn && TimerSeconds2 != PressOn)
                {
                    yield return null;
                }
                yield return FlipperSelectable;
                yield return null;
                yield return FlipperSelectable;
                yield break;
            }
        }
        yield break;
    }

    private System.Collections.IEnumerator MoveSwitch()
    {
        float moves = 0f;
        float newZ;
        FlipperMoving = true;
        if (InitMove)
        {
            moves = 1f;
        }
        while (moves <= 1.1f)
        {
            if (FlipperDown)
            {
                newZ = Mathf.Lerp(0.035f, -0.035f, moves);
            }
            else
            {
                newZ = Mathf.Lerp(-0.035f, 0.035f, moves);
            }
            FlipperPosition.localPosition = new Vector3(FlipperPosition.localPosition.x, FlipperPosition.localPosition.y, newZ);
            moves += Time.deltaTime * 2;
            yield return null;
        }
        FlipperMoving = false;
        InitMove = false;
    }

    protected void ModuleInit()
    {
        TopColor = UnityEngine.Random.Range(1, 7);
        BottomColor = UnityEngine.Random.Range(1, 7);

        string TopName = "none";
        string BottomName = "none";
        Color Orange = new Color(1f, 0.5f, 0f);
        Color Purple = new Color(0.75f, 0f, 0.75f);
        int RuleNumber;
        if (FlipperDown)
        {
            if (TopColor == 1 || BottomColor == 5)
            {
                NeededNumber = 5;
                RuleNumber = 1;
            }
            else if ((TopColor == 3 || TopColor == 4) && BombInfo.GetSerialNumberNumbers().Last() % 2 == 0)
            {
                NeededNumber = 3;
                RuleNumber = 2;
            }
            else if ((BottomColor == 3 || BottomColor == 4) && BombInfo.GetSerialNumberNumbers().Last() % 2 == 1)
            {
                NeededNumber = 6;
                RuleNumber = 3;
            }
            else if (TopColor == BottomColor)
            {
                NeededNumber = 0;
                RuleNumber = 4;
            }
            else
            {
                NeededNumber = 9;
                RuleNumber = 5;
            }
        }
        else
        {
            if ((TopColor == 6 || BottomColor == 6) && (BombInfo.IsPortPresent(KMBI.KnownPortType.RJ45)))
            {
                NeededNumber = 1;
                RuleNumber = 1;
            }
            else if (TopColor == 2 || BottomColor == 2)
            {
                NeededNumber = 4;
                RuleNumber = 2;
            }
            else if ((BottomColor == 1 || BottomColor == 3))
            {
                NeededNumber = 7;
                RuleNumber = 3;
            }
            else if (BombInfo.GetBatteryCount() > 1 && BombInfo.IsIndicatorOff(KMBI.KnownIndicatorLabel.TRN))
            {
                NeededNumber = 8;
                RuleNumber = 4;
            }
            else
            {
                NeededNumber = 2;
                RuleNumber = 5;
            }
        }


        if (TopColor == 1)
        {
            TopLED.material.color = Color.red;
            TopName = "red";
        }
        else if (TopColor == 2)
        {
            TopLED.material.color = Orange;
            TopName = "orange";
        }
        else if (TopColor == 3)
        {
            TopLED.material.color = Color.yellow;
            TopName = "yellow";
        }
        else if (TopColor == 4)
        {
            TopLED.material.color = Color.green;
            TopName = "green";
        }
        else if (TopColor == 5)
        {
            TopLED.material.color = Color.blue;
            TopName = "blue";
        }
        else
        {
            TopLED.material.color = Purple;
            TopName = "purple";
        }

        if (BottomColor == 1)
        {
            BottomLED.material.color = Color.red;
            BottomName = "red";
        }
        else if (BottomColor == 2)
        {
            BottomLED.material.color = Orange;
            BottomName = "orange";
        }
        else if (BottomColor == 3)
        {
            BottomLED.material.color = Color.yellow;
            BottomName = "yellow";
        }
        else if (BottomColor == 4)
        {
            BottomLED.material.color = Color.green;
            BottomName = "breen";
        }
        else if (BottomColor == 5)
        {
            BottomLED.material.color = Color.blue;
            BottomName = "blue";
        }
        else
        {
            BottomLED.material.color = Purple;
            BottomName = "purple";
        }

        if (SOLVED)
        {
            BottomLED.material.color = Color.black;
            TopLED.material.color = Color.black;
        }
        else
        {
            DebugLog("Switch is {0}. Top LED is {1}, bottom LED is {2}", FlipperDown ? "down" : "up", TopName, BottomName);
            DebugLog("Rule is {0}, number needed is {1}", RuleNumber.ToString(), NeededNumber.ToString());
        }
        StartCoroutine(MoveSwitch());
    }

    protected void Start()
    {
        int FlipInt = UnityEngine.Random.Range(0, 2);
        if (FlipInt == 1)
        {
            FlipperDown = !FlipperDown;
        }
        Switch_moduleId = Switch_moduleIdCounter++;
        FlipperSelectable.OnInteract += HandlePress;
        ModuleInit();
    }

    protected bool HandlePress()
    {
        if (!SOLVED && !FlipperMoving)
        {
            KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, transform);
            FlipperSelectable.AddInteractionPunch(0.5f);
            FlipperDown = !FlipperDown;
            if (TimerSeconds1 == NeededNumber || TimerSeconds2 == NeededNumber)
            {
                if (FirstSuccess)
                {
                    BombModule.HandlePass();
                    SOLVED = true;
                    TopLED.material.color = Color.black;
                    BottomLED.material.color = Color.black;
                }
                else
                {
                    FirstSuccess = true;
                }
            }
            else
            {
                BombModule.HandleStrike();
                FirstSuccess = false;
            }
            ModuleInit();
        }

        return false;
    }

    private void Update()
    {
        if (TimerSeconds2 != (int)(BombInfo.GetTime()) % 10)
        {
            TimerSeconds1 = ((int)(BombInfo.GetTime()) % 60 - (int)(BombInfo.GetTime()) % 10) / 10;
            TimerSeconds2 = (int)(BombInfo.GetTime()) % 10;
        }
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[The Switch #{0}]: {1}", Switch_moduleId, logData);
    }
}