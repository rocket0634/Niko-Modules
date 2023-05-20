using UnityEngine;
partial class FaultySink
{
    private void RunOverheat()
    {
        rotate = false;
        KMSelectable H = Hot;
        if (Random.Range(0, 2) < 1)
        {
            _scaffold.Hot = Faucet;
            _selectableChildren[1] = Faucet;
        }
        else
        {
            _scaffold.Hot = Pipe;
            _selectableChildren[7] = Pipe;
        }
        UpdateChildren();
        // Carefully remove the Hot knob from its base functions, while still allowing the knob to be interacted.
        SwapHot(H);
        // Reassign the Hot selectable to a new function
        H.OnInteract += delegate
        {
            Strike("Warning: Overheating");
            processingInput = false;
            return false;
        };
        ColKnobs = 0;
        ColFaucet = 0;
        ColPipe = 2;
        // Recalculate ColKnobs == 2, ColFaucet == 1, and ColPipe == 0
        for (int i = 2; i < 5; i++)
            Rules[i] = false;
        foreach (MeshRenderer render in ren)
            render.material.color = Color.black;
        base.Steps();
    }

    private void SwapHot(KMSelectable H)
    {
        // Switch the knob in the container
        knobs[H].knob = Hot;
        // Copy the Hot container to the new selectable
        knobs.Add(Hot, knobs[H]);
        // Make sure the new selectable gets assigned properly
        Handlers.Add(Hot, () => ButtonHandler(Hot));
        // Remove the previous container
        knobs.Remove(H);
        Handlers.Remove(H);
        // Rename the selectable for logging purposes.
        Hot.name = "Hot";
    }
}