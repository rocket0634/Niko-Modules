using System.Linq;
using UnityEngine;
public static class VersionHelper
{
    private static string _version;
    internal static string Version(this KMModSource source)
    {
        if (Application.isEditor)
        {
            Debug.Log("check \"Configure Mod\"");
            return "NA";
        }
        return _version ?? (_version = GetVersion(source));
    }

    private static string GetVersion(KMModSource source)
    {
        var applicableDirs = ModManager.Instance.InstalledModInfos.Where(x => (x.Value != null) && (x.Value.ID != null) && (x.Value.ID == source.ModID) && (x.Value.Version != null));
        if (applicableDirs.Count() < 1)
            return "NA";
        else if (applicableDirs.Count() > 1)
        {
            Debug.LogFormat("[Version Getter] There is more than one mod installed on this machine with the modID \"{0}\"]:\n"
            + "{1}", source.ModID, applicableDirs.Select(x => x.Key + ": Version " + x.Value.Version).Join("\n"));
            return "NA";
        }
        return applicableDirs.First().Value.Version;
    }
}