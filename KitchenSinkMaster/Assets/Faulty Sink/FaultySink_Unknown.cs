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
        var step = curKnob == 1;
        if (knob == Basin && step)
            Solve();
        else if (knob == Hot && !step)
        {
            Hot.transform.Rotate(Vector3.up, -50f);
            curKnob++;
        }
        else
        {
            Module.HandleStrike();
            if (knob != Cold)
                Log("Warning: Invalid button [{0}Mesh] selected.", knob.name);
            processingInput = false;
        }
        return false;
    }
}