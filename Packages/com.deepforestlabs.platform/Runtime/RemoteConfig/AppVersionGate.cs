#nullable enable
using System;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Semver compare for force-update: update required when <paramref name="currentVersion"/> &lt; <paramref name="minRequiredVersion"/>.
    /// </summary>
    public static class AppVersionGate
    {
        public static bool IsUpdateRequired(string? currentVersion, string? minRequiredVersion)
        {
            if (!TryParse(currentVersion, out Version current))
            {
                return false;
            }

            if (!TryParse(minRequiredVersion, out Version min))
            {
                return false;
            }

            return current < min;
        }

        public static bool TryParse(string? version, out Version parsed)
        {
            parsed = new Version(0, 0, 0);
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            string trimmed = version.Trim();
            // Allow "1.0.0f1" / "1.0.0-preview" → take numeric dotted prefix.
            int cut = trimmed.Length;
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if (char.IsDigit(c) || c == '.')
                {
                    continue;
                }

                cut = i;
                break;
            }

            string numeric = trimmed.Substring(0, cut).TrimEnd('.');
            if (string.IsNullOrEmpty(numeric))
            {
                return false;
            }

            // System.Version needs at least major.minor
            string[] parts = numeric.Split('.');
            if (parts.Length == 1)
            {
                numeric += ".0";
            }

            return Version.TryParse(numeric, out parsed);
        }
    }
}
#nullable disable
