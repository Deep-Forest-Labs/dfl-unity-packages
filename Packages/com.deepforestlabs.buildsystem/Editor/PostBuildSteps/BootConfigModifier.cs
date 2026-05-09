using System.IO;
using DeepForestLabs.Logger;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PostBuildSteps
{
    public sealed class BootConfigModifier : IPostprocessBuildWithReport
    {
        public int callbackOrder => (int)PostBuildOrder.BootConfigModifier;

        public void OnPostprocessBuild(BuildReport report)
        {
            string bootConfigPath = Path.Combine(report.summary.outputPath, "boot.config");

            if (File.Exists(bootConfigPath))
            {
                BuildLog.Info("Modifying boot.config at: {0}", bootConfigPath);

                // Read existing config and modify
                var lines = File.ReadAllLines(bootConfigPath);
                bool foundSetting = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("player-background-worker-count="))
                    {
                        lines[i] = "player-background-worker-count=16"; // Adjust worker count
                        foundSetting = true;
                        break;
                    }
                }

                if (!foundSetting)
                {
                    File.AppendAllText(bootConfigPath, "\nplayer-background-worker-count=8\n");
                }
                else
                {
                    File.WriteAllLines(bootConfigPath, lines);
                }

                BuildLog.Info("Successfully set player-background-worker-count to 8.");
            }
        }
    }
}