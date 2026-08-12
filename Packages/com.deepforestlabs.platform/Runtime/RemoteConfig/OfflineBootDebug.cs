#nullable enable
using UnityEngine;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Debug-only escape hatch for boot gate. Compiled out of release behavior via <c>NOT_RELEASE_BUILD</c>.
    /// </summary>
    public static class OfflineBootDebug
    {
        public const string PrefsKey = "dfl.debug.allow_offline_boot";

        public static bool IsAllowed
        {
            get
            {
#if NOT_RELEASE_BUILD
                return PlayerPrefs.GetInt(PrefsKey, 0) != 0;
#else
                return false;
#endif
            }
        }

        public static void SetAllowed(bool allowed)
        {
#if NOT_RELEASE_BUILD
            PlayerPrefs.SetInt(PrefsKey, allowed ? 1 : 0);
            PlayerPrefs.Save();
#else
            _ = allowed;
#endif
        }
    }
}
#nullable disable
