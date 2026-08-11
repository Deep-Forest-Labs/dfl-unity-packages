#nullable enable
using System.Collections.Generic;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;

namespace DeepForestLabs.BuildSystems.PlatformSetup
{
    public sealed class iOSBuildSetup : IPlatformBuildSetup
    {
        public BuildTarget Target => BuildTarget.iOS;
        public NamedBuildTarget NamedTarget => NamedBuildTarget.iOS;

        public void ConfigureProjectSettings(CommandLineArgs args)
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.iOS, Il2CppCodeGeneration.OptimizeSize);

            string? teamId = SigningEnvironment.Get(SigningEnvironment.AppleTeamId);
            if (!string.IsNullOrEmpty(teamId))
            {
                PlayerSettings.iOS.appleDeveloperTeamID = teamId;
                PlayerSettings.iOS.appleEnableAutomaticSigning = true;
                BuildLog.Info("iOSBuildSetup - Applied Apple Team ID from {0}", SigningEnvironment.AppleTeamId);
            }
            else if (args.IsCommandLineBuild && args.IsTestFlightBuild)
            {
                if (string.IsNullOrWhiteSpace(PlayerSettings.iOS.appleDeveloperTeamID))
                {
                    throw new BuildException(
                        $"iOS TestFlight/CI builds require env var {SigningEnvironment.AppleTeamId} "
                        + "or a Team ID already set in Player Settings. See docs/build-system.md.");
                }
            }

            AssetDatabase.SaveAssets();
        }

        public string GetOutputPath(CommandLineArgs args, string baseBuildPath)
        {
            string outputLocation = baseBuildPath + "xcodefiles";
            if (!System.IO.Directory.Exists(outputLocation))
            {
                System.IO.Directory.CreateDirectory(outputLocation);
            }
            return outputLocation;
        }

        public int GetBuildNumber()
        {
            return int.TryParse(PlayerSettings.iOS.buildNumber, out int num) ? num : 0;
        }

        public void SetBuildNumber(int buildNumber)
        {
            PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        }

        public Dictionary<string, string> GetDefaultPlatformArgs()
        {
            return new Dictionary<string, string>();
        }
    }
}
#nullable disable
