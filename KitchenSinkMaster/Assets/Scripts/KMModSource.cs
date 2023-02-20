using UnityEngine;
/// <summary>
/// Use this component to gain access to your project's mod identification (modID) from within Keep Talking and Nobody Explodes.
/// </summary>
/// <remarks>
/// <para>Mods that share the same ModID are not supported, however, the game does not have any protections against doing so. Be careful to make sure the ModID of your mod doesn't conflict with another while developing your mod.</para>
/// <para>This component is required to be attached to a gameobject in order to function properly.</para>
/// </remarks>
public class KMModSource : MonoBehaviour
{
    private static string _modID;
    /// <returns>
    /// The identifer for your mod. If the ModSource component is not available or if this is called in the Editor, it will instead return the calling assembly's name.
    /// </returns>
    internal string ModID { 
        get
        {
            return _modID ?? GetModID();
        } 
    }

    private string GetModID()
    {
        ModSource source = GetComponent<ModSource>();
        // Note that this returns the calling assembly's name. This may be different from your mod's id if you are using an external library for calling this property.
        _modID = GetType().Assembly.GetName().Name;
        if (source != null && !string.IsNullOrEmpty(source.ModName))
            _modID = source.ModName;
        return _modID;
    }
}