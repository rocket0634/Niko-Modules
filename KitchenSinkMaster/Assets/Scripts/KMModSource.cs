using Assets.Scripts.Mods;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Use this component to gain access to your project's mod identification (modID) from within Keep Talking and Nobody Explodes.
/// </summary>
/// <remarks>
/// <para>Mods that share the same ModID are not supported, however, the game does not have any protections against doing so. Be careful to make sure the ModID of your mod doesn't conflict with another while developing your mod.</para>
/// <para>This component is recommended to be attached to a prefab tagged as mod.bundle in order to function properly.</para>
/// </remarks>
public class KMModSource : MonoBehaviour
{
    private static string _modID;
    private static IEnumerable<Mod> _mods;
    private static string _modPath;
    private static string _version;
    private static ModManager Manager = ModManager.Instance;
    /// <returns>
    /// The identifer for your mod. If the ModSource component is not available or if this is called in the Editor, it will instead return the calling assembly's name.
    /// </returns>
    internal string ModID { 
        get
        {
            return _modID ?? GetModID();
        } 
    }

    /// <returns>
    /// Obtains the path used to load this mod. This can only be guaranteed if this component is attached to a prefab that is not instantiated. Otherwise, it will return null.
    /// </returns>
    internal string ModPath
    {
        get
        {
            return _modPath ?? GetModPath();
        }
    }
    internal string Version()
    {
        if (Application.isEditor)
        {
            Debug.Log("check \"Configure Mod\"");
            return "NA";
        }
        return _version ?? (_version = GetVersion());
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

    private string GetModPath()
    {
        var applicableDirs = Manager.loadedMods.Where(x => x.Value.ModID == ModID);
        _mods = applicableDirs.Count() > 0 ? applicableDirs.Select(x => x.Value) : new List<Mod>();
        var dir = _mods.FirstOrDefault(x => x.ModObjects != null && x.ModObjects.Contains(gameObject));
        _modPath = dir == default(Mod) ? null : dir.modDirectory;
        return _modPath;
    }

    private string GetVersion()
    {
        // This only works if KMModSource is directly referenced from a prefab with mod.bundle
        // If the gameobject is instantiated, obviously the game object won't be the same.
        // Using this can guarantee which mod we loaded in from, without any guessing.
        var dir = ModPath ?? _mods.First().modDirectory;
        var applicableInfos = _mods.ToDictionary(x => x.modDirectory, x =>
        {
            ModInfo y;
            // InstalledModInfos isn't written to by Tweaks, and won't work if workshop mods are loaded locally.
            if (!Manager.InstalledModInfos.TryGetValue(x.modDirectory, out y))
                Manager.GetModInfoFromPath(x.modDirectory, 0);
            return y;
        });
        if (applicableInfos.Count < 1)
            return "NA";
        else if (applicableInfos.Count() > 1)
        {
            Debug.LogFormat("[KMModSource] There is more than one mod installed on this machine with the modID \"{0}\"]: \n"
            + "{1}", _modID, applicableInfos.Select(x => x.Key + ": Version " + x.Value.Version).Join("\n"));
        }
        var info = applicableInfos[dir];
        return _version = info.Version ?? "NA";
    }
}