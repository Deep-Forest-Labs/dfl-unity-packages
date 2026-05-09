#nullable enable
using DeepForestLabs.Utils;
using UnityEngine;

namespace IOSSettingsBundle
{
    public static class SettingsBundle
    {
        #region -- Public Properties --

        public static string PlayerID
        {
            get { return PrefUtils.GetString("PlayerID", string.Empty); }
            set { PrefUtils.SetString("PlayerID", value); }
        }

        public static string PlayerName
        {
            set { PrefUtils.SetString("PlayerName", value); }
        }

        public static string? INK_ID
        {
            set
            {
                if (value == null)
                {
                    PrefUtils.DeleteKey("INKID");
                    return;
                }
                PrefUtils.SetString("INKID", value);
            }
        }

        public static string AppVersion
        {
            set { PrefUtils.SetString("AppVersion", value); }
        }

        public static void ClearUserInfo()
        {
            PlayerPrefs.DeleteKey("INKID");
            PlayerPrefs.DeleteKey("PlayerName");
            PlayerPrefs.DeleteKey("PlayerID");
        }

        #endregion
    }
}
#nullable disable