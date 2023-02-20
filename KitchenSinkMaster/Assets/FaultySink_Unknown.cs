using System;
using System.Linq;
using UnityEngine;
partial class FaultySink
{
    private KMSelectable Basin { get { return _scaffold.Basin; } }
    private void RunUnknown()
    {
        rotate = false;
        PipeMesh.material.color = Color.black;
        base.ApplyTextures(Enumerable.Range(0, 3));
        _selectableChildren[4] = Basin;
        UpdateChildren();
        UpdateHandlers(true, FaultyPVC, Basin);
        Log("Module status is unknown. Please submit [Hot] [Basin] to automatically disable the module.");
    }

    private bool FaultyPVC(KMSelectable knob)
    {
        var num = Array.IndexOf(new[] { Hot, Basin, Cold }, knob);
        switch(num + curKnob * 3)
        {
            // 0 + 0
            case 0:
                Hot.transform.Rotate(Vector3.up, -50f);
                curKnob++;
            break;
            // 1 + 0
            case 1:
                Module.HandleStrike();
                Log("Warning: Invalid button [BasinMesh] selected.");
            break;
            // 2 + 0, 2 + 3
            case 2:
            case 5:
                Module.HandleStrike();
            break;
            // 0 + 3
            case 3:
                Module.HandleStrike();
                Log("Warning: Invalid button [HotMesh] selected.");
            break;
            // 1 + 3
            case 4:
                Solve();
            break;
        }
        processingInput = false;
        return false;
    }
}