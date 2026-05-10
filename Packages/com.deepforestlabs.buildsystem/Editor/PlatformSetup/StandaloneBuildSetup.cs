#nullable enable
using System.Collections.Generic;
using System.IO;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.PlatformSetup
{
    public sealed class StandaloneBuildSetup : IPlatformBuildSetup
    {
        public BuildTarget Target => BuildTarget.StandaloneWindows64;
        public NamedBuildTarget NamedTarget => NamedBuildTarget.Standalone;

        public void ConfigureProjectSettings(CommandLineArgs args)
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSize);
            AssetDatabase.SaveAssets();
        }

        public string GetOutputPath(CommandLineArgs args, string baseBuildPath)
        {
            string productName = PlayerSettings.productName.Replace(" ", string.Empty);
            string fileName = ZString.Format("{0}_{1}_{2}_{3}.exe", productName, args.UniqueId, args.BuildNumber, args.Environment.ToLower());
            return Path.Combine(baseBuildPath, fileName);
        }

        public int GetBuildNumber()
        {
            return BuildSettings.Instance.BuildNumber;
        }

        public void SetBuildNumber(int buildNumber)
        {
            // Standalone has no dedicated build number field; value is stored in BuildSettings
        }

        public Dictionary<string, string> GetDefaultPlatformArgs()
        {
            return new Dictionary<string, string>();
        }
    }
}
#nullable disable
