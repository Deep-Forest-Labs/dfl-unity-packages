#nullable enable
using System;
using System.IO;
using System.Text;

namespace DeepForestLabs.BuildSystems
{
    /// <summary>
    /// CI signing values sourced from environment variables (GitHub Actions secrets → env).
    /// Never log secret values.
    /// </summary>
    public static class SigningEnvironment
    {
        public const string AndroidKeystorePath = "DFL_ANDROID_KEYSTORE_PATH";
        public const string AndroidKeystorePass = "DFL_ANDROID_KEYSTORE_PASS";
        public const string AndroidKeystorePassB64 = "DFL_ANDROID_KEYSTORE_PASS_B64";
        public const string AndroidKeyAlias = "DFL_ANDROID_KEY_ALIAS";
        public const string AndroidKeyAliasPass = "DFL_ANDROID_KEY_ALIAS_PASS";
        public const string AndroidKeyAliasPassB64 = "DFL_ANDROID_KEY_ALIAS_PASS_B64";
        public const string AppleTeamId = "DFL_APPLE_TEAM_ID";

        public static string? Get(string name)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public static string? GetPassword(string plainName, string base64Name)
        {
            string? b64 = Get(base64Name);
            if (b64 != null)
            {
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                }
                catch (FormatException e)
                {
                    throw new BuildException(
                        $"Environment variable '{base64Name}' is not valid base64.", e);
                }
            }

            return Get(plainName);
        }

        public static bool HasAndroidKeystoreConfig()
        {
            return Get(AndroidKeystorePath) != null
                   && GetPassword(AndroidKeystorePass, AndroidKeystorePassB64) != null
                   && Get(AndroidKeyAlias) != null
                   && GetPassword(AndroidKeyAliasPass, AndroidKeyAliasPassB64) != null;
        }

        public static void RequireAndroidKeystoreForStoreBuild()
        {
            if (!HasAndroidKeystoreConfig())
            {
                throw new BuildException(
                    "Android store/CI signing requires env vars: "
                    + $"{AndroidKeystorePath}, {AndroidKeystorePass} (or {AndroidKeystorePassB64}), "
                    + $"{AndroidKeyAlias}, {AndroidKeyAliasPass} (or {AndroidKeyAliasPassB64}). "
                    + "See docs/build-system.md.");
            }

            string path = Get(AndroidKeystorePath)!;
            if (!File.Exists(path))
            {
                throw new BuildException($"Android keystore file not found at '{path}'.");
            }
        }
    }
}
#nullable disable
