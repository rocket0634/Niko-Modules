using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
partial class FaultySink
{
    private bool processingInput;
    protected override IEnumerator ProcessTwitchCommand(string tpInput)
    {
        if (processingInput && !SOLVED)
        {
            yield return "sendtochaterror Still processing previous input, command dropped.";
            yield break;
        }
        string input = tpInput.ToLowerInvariant();
        if (input == "sinkcommands")
        {
            yield return "sendtochat Sink: " + SinkHelpMessage.Replace("{0}", "{1}") + " Manual: https://ktane.timwi.de/HTML/Sink.html";
            yield break;
        }
        if (faulty == 3 && input.EqualsAny("hover", "highlight"))
        {
            Animator highlightAnimator = Hot.Highlight.GetComponentInChildren<Animator>(true);
            // this will skip in the test harness
            if (highlightAnimator != null)
            {
                highlightAnimator.gameObject.SetActive(true);
                highlightAnimator.Play("InteractionPulse");
                yield return new WaitForSeconds(1.5f);
                highlightAnimator.gameObject.SetActive(false);
            }
#if UNITY_EDITOR
            TestHighlightable testHighlight = Hot.Highlight.GetComponentInChildren<TestHighlightable>(true);
            testHighlight.On();
            yield return new WaitForSeconds(1.5f);
            testHighlight.Off();
#endif      
            yield break;
        }
        var tp = base.ProcessTwitchCommand(tpInput);
        matches = "chpbsfd";
        // Cold, Hot, Pipe, Basin, (Sink) Basin, Faucet, (Drain) Pipe
        indicies = new[] { 0, 2, 7, 4, 4, 1, 7 };
        fullMatch = "^cold|hot|pipe|basin|sink|faucet|drain$";
        processingInput = true;
        while (tp.MoveNext())
        {
            var curObj = tp.Current;
            if (curObj is KMSelectable[])
            {
                List<KMSelectable> tapList = ((KMSelectable[])curObj).ToList();
                var fullCount = tapList.Count;
                while (tapList.Count != 0)
                {
                    var isRotating = rotating;
                    yield return new KMSelectable[] { tapList.First() };
                    tapList.RemoveAt(0);
                    if (tapList.Count > 0)
                         yield return null;
                    var processed = fullCount - tapList.Count;
                    var plural = processed != 1;
                    yield return new WaitUntil(() => canPress || rotating);
                    if (rotating && rule == 2)
                    {
                        Reset();
                        yield return new WaitUntil(() => !rotating);
                    }
                    if (rotating && !isRotating && tapList.Count > 0)
                    {
                        yield return string.Format("sendtochaterror Due to a sudden change, only {0} {1} processed.", processed, plural ? "inputs were" : "input was");
                        processingInput = false;
                        yield break;
                    }
                }
            }
            else yield return curObj;
        }
        processingInput = false;
    }

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        if (rotating)
            Reset();
        rotate = false;
        spin = 0;
        if (faulty == 0)
        {
            if (curKnob == 0)
            {
                Hot.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            Basin.OnInteract();
            yield break;
        }
        if (faulty == 2)
        {
            if (!o)
            {
                var broken = HotMesh.material.mainTexture == null || ColdMesh.material.mainTexture == null;
                var correct = ColKnobs == ColPipe ? new { k = Pipe, r = PipeMesh } : new { k = Faucet, r = FaucetMesh };
                var missing = HotMesh.material.mainTexture == correct.r.material.mainTexture ? new { k = Cold, r = ColdMesh } : new { k = Hot, r = HotMesh };
                if (broken) missing.r.material = null;
                while (!o)
                {
                    if (knob2Turn[0].temp)
                    {
                        missing.k.OnInteract();
                        yield return new WaitForSeconds(.1f);
                    }
                    else
                    {
                        correct.k.OnInteract();
                        yield return new WaitForSeconds(.1f);
                    }
                }
            }
        }
        yield return base.TwitchHandleForcedSolve();
    }
}