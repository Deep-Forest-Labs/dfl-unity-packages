#nullable enable
using System.Collections.Generic;
using System.IO;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.PlatformSetup
{
    public sealed class WebGLBuildSetup : IPlatformBuildSetup
    {
        public BuildTarget Target => BuildTarget.WebGL;
        public NamedBuildTarget NamedTarget => NamedBuildTarget.WebGL;

        public void ConfigureProjectSettings(CommandLineArgs args)
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);

            if (args.PlatformArgs.TryGetValue("compressionFormat", out string? format))
            {
                PlayerSettings.WebGL.compressionFormat = format switch
                {
                    "brotli" => WebGLCompressionFormat.Brotli,
                    "gzip" => WebGLCompressionFormat.Gzip,
                    _ => WebGLCompressionFormat.Disabled
                };
            }

            AssetDatabase.SaveAssets();
        }

        public string GetOutputPath(CommandLineArgs args, string baseBuildPath)
        {
            string productName = PlayerSettings.productName.Replace(" ", string.Empty);
            string folderName = ZString.Format("{0}_{1}_{2}_{3}", productName, args.UniqueId, args.BuildNumber, args.Environment.ToLower());
            return Path.Combine(baseBuildPath, folderName);
        }

        public int GetBuildNumber()
        {
            return BuildSettings.Instance.BuildNumber;
        }

        public void SetBuildNumber(int buildNumber)
        {
            // WebGL has no native build number field; value is stored in BuildSettings
        }

        public Dictionary<string, string> GetDefaultPlatformArgs()
        {
            return new Dictionary<string, string>
            {
                ["compressionFormat"] = "brotli"
            };
        }
    }
}
#nullable disable
