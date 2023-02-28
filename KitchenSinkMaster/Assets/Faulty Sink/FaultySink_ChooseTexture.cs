using System.Collections.Generic;
using System.Linq;
using UnityEngine;
partial class FaultySink
{
    // The texture the user is holding when fixing the material
    private Texture texHold;
    // The material to apply to the broken knob when it has been fixed
    private Material buttonMasher;
    // Tell the autosolver if we are out of the texture function or not
    private bool o;
    private void RunSetupTexture()
    {
        var r = Random.Range(0, 2);
        _selectableChildren[1] = Faucet;
        _selectableChildren[7] = Pipe;
        UpdateChildren();
        while (new [] { ColFaucet, ColPipe }.Count(x => x == ColKnobs) != 1)
        {
            ColFaucet = Random.Range(0, 3);
            ColPipe = Random.Range(0, 3);
        }
        base.ApplyTextures(Enumerable.Range(0, 3));
        PipeMesh.material.mainTexture = _scaffold.KnobColors[ColPipe];
        if (r == 0) ColdMesh.material = null;
        else HotMesh.material = null;
        UpdateHandlers(true, x => ChooseTexture(x, r), Pipe, Faucet);
        Rules[3] = ColFaucet.Equals(1);
        Rules[4] = ColPipe.Equals(0);
        knob2Turn = Enumerable.Range(0, 3).Select(x => new Temp()).ToArray();
        Log("Debug: Cannot determine status of Button{0}. Please select one of the following textures:", r);
        Log("Knobs: {0}", _scaffold.KnobColors[ColKnobs].name);
        Log("Faucet: {0}", FaucetMesh.material.mainTexture.name);
        Log("Drain Pipe: {0}", PipeMesh.material.mainTexture.name);
    }
    private bool ChooseTexture(KMSelectable selectable, int r)
    {
        var selectables = new List<KMSelectable> { Cold, Hot, Faucet, Pipe };
        var s = selectables.IndexOf(selectable);
        if (r != s && !knob2Turn[0].temp && s < 2)
        {
            Module.HandleStrike();
            processingInput = false;
            Log("NullReferenceException: Object reference not set to an instance of an object.");
        }
        else if (r == s && !knob2Turn[0].temp)
        {
            Module.HandleStrike();
            processingInput = false;
            Log("Warning: Invalid button [{0}] selected.", ren[(r + 1) % 2].material.mainTexture);
        }
        else if (!knob2Turn[0].temp)
        {
            texHold = ren[s].material.mainTexture;
            buttonMasher = ren[s].material;
            Log("Texture [{0}] chosen.", texHold.name);
            knob2Turn[0].temp = true;
        }
        else if (r == s)
        {
            ren[s].material.mainTexture = texHold;
            knob2Turn[0].temp = false;
            knob2Turn[1].temp = true;
            Log("Texture [{0}] applied.", texHold.name);
        }
        else
        {
            knob2Turn[0].temp = false;
            Log("Warning: Texture {0} applied.", s < 2 ? "[" + texHold.name + "] cannot be" : "already");
        }

        if (knob2Turn[1].temp)
        {
            if (ColdMesh.material.mainTexture == HotMesh.material.mainTexture)
            {
                ren[s].material = buttonMasher;
                _selectableChildren[1] = null;
                _selectableChildren[7] = null;
                UpdateChildren();
                // Unsubscribe from OnInteract
                UpdateHandlers(false);
                // Reset the cached handlers
                Handlers.Clear();
                // Re-add the Cold and Hot handlers, as they're set in Awake
                Handlers.Add(Cold, () => ButtonHandler(Cold));
                Handlers.Add(Hot, () => ButtonHandler(Hot));
                knobs.Remove(Faucet);
                knobs.Remove(Pipe);
                o = true;
                // OnInteract gets reassigned here
                base.Steps();
            }
            else knob2Turn[1].temp = false;
        }
        return false;
    }
}