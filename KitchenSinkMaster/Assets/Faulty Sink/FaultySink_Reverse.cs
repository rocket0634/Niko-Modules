using System.Linq;
using UnityEngine;
partial class FaultySink
{
    private void RunReverse()
    {
        _scaffold.transform.Rotate(0, 180, 0);
        base.ApplyTextures(Enumerable.Range(0, 4).Reverse());
        Rules = Rules.Reverse().ToArray();
        base.Steps();
    }

    protected override void DoAssigns(System.Action[] assign)
    {
        if (faulty == 4)
        {
            assign = assign.Reverse().ToArray();
            selectedRules = selectableRules[3 - Mathf.Clamp(batteries, 0, 3)];
            // Set Knob2Turn so that the logging doesn't fail
            knob2Turn = selectedRules.Select(x => Rules[x] ? knobs[Hot] : knobs[Cold]).ToArray();
            base.DoAssigns(assign);
            knob2Turn = knob2Turn.Reverse().ToArray();
            return;
        }
        base.DoAssigns(assign);
    }
}