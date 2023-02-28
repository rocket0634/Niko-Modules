using System.Collections;
using UnityEngine;
partial class FaultySink
{
    public float seconds;
    private bool rotate, rotating, holding;
    [SerializeField]
    private int spin, rule;
    private float dT;
    private Coroutine Timer;
    private void RunSpin()
    {
        spin = spin + Random.Range(0, 20);
        if (spin > 10)
            StartCoroutine(Spin());
    }
    private IEnumerator Spin()
    {
        rotate = true;
        while (rotate)
        {
            spin = Random.Range(0, 2000);
            yield return new WaitForSeconds(seconds);
        }
    }
    private IEnumerator Spinning(KMSelectable r)
    {
        var t = rule == 3 ? -2.5f : 2.5f;
        rotating = true;
        while (isActiveAndEnabled)
        {
            r.transform.Rotate(Vector3.up, t);
            yield return null;
        }
    }
    private IEnumerator Countdown()
    {
        dT = 0;
        while (holding)
        {
            dT++;
            yield return new WaitForSeconds(1);
            if (dT > 10) yield break;
        }
    }
    protected override IEnumerable ButtonPress(Temp t)
    {
        // This is set in the base function but not setting it here may break TP
        canPress = false;
        if (t == knob2Turn[curKnob] && rotate && !rotating)
        {
            var rot = new System.Func<bool>[] 
            {
                () => curKnob == 2 && spin > 1500,
                () => spin > 1295,
                () => spin > 900
            };
            for (int i = 0; i < rot.Length; i++)
            {
                if (rot[i]())
                {
                    var knob = t.knob;
                    knob.AddInteractionPunch(0.5f);
                    Audio.PlaySoundAtTransform("valve_spin", knob.transform);
                    yield return KnobTurn(t, 2.5f);
                    rule = i + 1;
                    WaitForSol();
                    yield break;
                }
                // Solve the module if the sequence rule isn't called.
                else if (curKnob == 2) break;
            }
        }
        foreach (var item in base.ButtonPress(t))
            yield return item;
        // Twitch Plays may be halted after a strike, causing processingInput to not be set to false.
        if (curKnob == 0)
            processingInput = false;
    }
    private void WaitForSol()
    {
        canPress = false;
        var k = new[] { Hot, Cold };
        var r = Random.Range(0, 2);
        StartCoroutine(Spinning(k[r]));
        UpdateHandlers(false);
        switch (rule)
        {
            case 1:
                Log("Debug: Rotation error, reverse the inputs to disable.");
                curKnob = 0;
                UpdateHandlers(true, x => Sequence(x));
                break;
            case 2:
                if (!processingInput) Log("Button not responding, please hold the button still to reorientate the button.");
                else Debug.LogFormat("<Faulty Sink {0}> Rotation Rule 2 applied, but ignored by Twitch Plays.", _moduleId);
                UpdateHandlers(true, x => StartCountdown(k[r] == x));
                k[r].OnInteractEnded += WaitForSelect;
                break;
            case 3:
                Log("Invalid button selected, please select a valid button.");
                UpdateHandlers(true, x => Opposite(k[r] == x));
                break;
        }
    }
    private bool Sequence(KMSelectable k)
    {
        Log("Debug: Pressing {0}", k.name);
        if (knob2Turn[2 - curKnob].knob == k)
        {
            if (curKnob < 2) curKnob++;
            else 
            {
                UpdateHandlers(false);
                Solve();
            }
        }
        else
        {
            curKnob = 0;
            Log("Incorrect sqeuence, resetting.");
        }
        return false;
    }
    private bool Opposite(bool correct)
    {
        if (correct)
            Reset();
        else
        {
            Module.HandleStrike();
            processingInput = false;
        }
        return false;
    }
    private bool StartCountdown(bool correct)
    {
        if (correct)
        {
            holding = true;
            Timer = StartCoroutine(Countdown());
        }
        return false;
    }
    private void WaitForSelect()
    {
        if (dT >= 3 && dT <= 5)
            Reset();
        else if (rule == 2)
        {
            StopCoroutine(Timer);
            if (spin > 1500 && dT > 5) Module.HandleStrike();
            Log("Debug: Reset failed after {0} seconds.", dT);
        }
        // WaitForSelect can be called after selecting a knob before the spinning rule activates
        // This would result in WaitForSelect being called before the Timer starts.
        else if (holding) StopCoroutine(Timer);
        holding = false;
    }
    private void Reset()
    {
        canPress = true;
        StopAllCoroutines();
        StartCoroutine(CheckForTurn());
        if (rotate)
            StartCoroutine(Spin());
        // Remove rotation interaction
        UpdateHandlers(false);
        // Restore previous function
        UpdateHandlers(true, x => ButtonHandler(x));
        Cold.OnInteractEnded -= WaitForSelect;
        Hot.OnInteractEnded -= WaitForSelect;
        rotating = false;
        curKnob++;
        if (!processingInput) Log("Button reoriented");
        else Debug.LogFormat("<Faulty Sink #{0}> Button reoriented", _moduleId);
    }
}