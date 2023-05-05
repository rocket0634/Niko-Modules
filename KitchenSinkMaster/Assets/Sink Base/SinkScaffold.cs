using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkScaffold : MonoBehaviour {
	#pragma warning disable 649 // Stop compiler warning: "Field is not assigned to" (they are assigned in Unity)
	public KMSelectable Cold, Hot;
	public Texture[] KnobColors;
    public KMModSource ModSource;
    
    #region Faulty
    public KMSelectable Faucet, Pipe, Basin;
    public MeshRenderer ColdMesh, HotMesh, FaucetMesh, PipeMesh;
    #endregion
    #pragma warning restore 649
}
