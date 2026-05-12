using System.IO;
using System.Linq;
using DeepForestLabs.Logger;
using ZLinq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
    public sealed class ClientPreBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => (int)PreBuildOrder.ClientPreBuilder;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            BuildSettings buildSettings = BuildSettings.Instance;

            // Validate scenes
            string[] scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();

            // Log info
            BuildLog.Info("Performing Client Build");
            string arg2 = buildSettings.IsDebugBuild ? "(DEBUG)" : "(NOT DEBUG)";
            BuildLog.Info("Building {0} {1} for {2}", EditorUserBuildSettings.activeBuildTarget, arg2, buildSettings.Environment);
            BuildLog.Info("Release Build: {0}, Build Number: {1}", buildSettings.IsReleaseBuild, buildSettings.BuildNumber);
            BuildLog.Info("Unique ID: {0}", buildSettings.Addressables.UniqueId);
            BuildLog.Info("Total Scenes to Build: {0}", scenes.Length);
            string arg1 = string.Join(",", scenes);
            BuildLog.Info("Included Scenes: {0}", arg1);
            BuildLog.Info("Output Path: \"{0}\"", report.summary.outputPath);

            //Pre Build
            BuildLog.Info("BuildPlayer phase prebuild...");
            // Backup any files that get modified as a result of doing a build
            PreBuildBackupFiles();
            CreateBuildOutputDirectory();
            // Set splash off and refresh asset database since files moved around
            PlayerSettings.SplashScreen.show = false;
            AssetDatabase.SaveAssets();

            // Build the client
            BuildLog.Info("BuildPlayer phase starting...");
        }

        private void CreateBuildOutputDirectory()
        {
            string path = BuilderUtils.GetBuildPath();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void PreBuildBackupFiles()
        {
            int count = 0;
            FilesToCleanup filesToCleanup = AssetDatabase.LoadAssetAtPath<FilesToCleanup>(FilesToCleanup.ASSET);

            if (filesToCleanup.FilesToBackupAndRestore.Count > 0)
            {
                // Remove any existing files and directories
                string backupPath = BuilderUtils.GetBackupPath();
                if (Directory.Exists(backupPath))
                {
                    Directory.Delete(backupPath, true);
                }
                Directory.CreateDirectory(backupPath);

                BuildLog.Info("Backing up files.");
                foreach (var relativePathItem in filesToCleanup.FilesToBackupAndRestore)
                {
                    string dstPath = Path.GetFullPath(Path.Combine(backupPath, Path.GetDirectoryName(relativePathItem) ?? string.Empty));
                    string srcFile = Path.Combine(Application.dataPath, relativePathItem);
                    string dstFile = Path.Combine(dstPath, Path.GetFileName(relativePathItem));

                    if (!Directory.Exists(dstPath))
                    {
                        Directory.CreateDirectory(dstPath);
                    }

                    BuildLog.Info("Backing up {0} to {1}", relativePathItem, dstFile);
                    if (File.Exists(srcFile))
                    {
                        File.Copy(srcFile, dstFile, true);
                        count++;
                    }
                }
                BuildLog.Info("Backed up {0} files total.", count);
            }
        }
    }
}