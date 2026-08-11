#nullable enable
using UnityEngine;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Lifetime-once flags for funnel events. Stored in PlayerPrefs (not game save).
    /// </summary>
    public static class AnalyticsOnce
    {
        private const string Prefix = "dfl.analytics.once.";

        public static bool HasClaimed(string key) =>
            PlayerPrefs.GetInt(Prefix + key, 0) != 0;

        /// <summary>
        /// Returns true the first time <paramref name="key"/> is claimed; false thereafter.
        /// </summary>
        public static bool TryClaim(string key)
        {
            string prefsKey = Prefix + key;
            if (PlayerPrefs.GetInt(prefsKey, 0) != 0)
            {
                return false;
            }

            PlayerPrefs.SetInt(prefsKey, 1);
            PlayerPrefs.Save();
            return true;
        }
    }
}
#nullable disable
