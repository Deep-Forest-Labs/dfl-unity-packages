#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.PostBuildSteps
{
    public sealed class ClientPostBuilder : IPostprocessBuildWithReport, IPreprocessBuildWithReport
    {
	    private static Stopwatch Stopwatch { get; set; } = new();
	    
        public int callbackOrder => (int)PostBuildOrder.ClientPostBuilder;
        public void OnPreprocessBuild(BuildReport report)
        {
	        Stopwatch = new Stopwatch();
	        Stopwatch.Start();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
	        //Post Build
	        BuildLog.Info("BuildPlayer phase post build...");
	        FilesToCleanup filesToCleanup = AssetDatabase.LoadAssetAtPath<FilesToCleanup>(FilesToCleanup.ASSET);
	        PostBuildRestoreFiles(filesToCleanup);
	        PostBuildDeleteFiles(filesToCleanup);

	        Stopwatch.Stop();
	        
	        // Validate and Exit
	        ValidateResults(report, Stopwatch.Elapsed);
        }
        
        private void PostBuildRestoreFiles(FilesToCleanup filesToCleanup)
        {
        	int count = 0;
            
        	if (filesToCleanup.FilesToBackupAndRestore.Count > 0)
        	{
        		string backupPath = BuilderUtils.GetBackupPath();
        		BuildLog.Info("Restoring files.");
        		foreach (var relativePathItem in filesToCleanup.FilesToBackupAndRestore)
        		{
        			string srcFile = Path.GetFullPath(Path.Combine(backupPath, relativePathItem));
        			string dstFile = Path.Combine(Application.dataPath, relativePathItem);
    
                    BuildLog.Info("Restoring {0} to {1}", srcFile, dstFile);
        			if (File.Exists(srcFile))
        			{
        				File.Copy(srcFile, dstFile, true);
        				count++;
        			}
                }
        		BuildLog.Info("Restored {0} files total.", count);
        	}
        }
    
        private void PostBuildDeleteFiles(FilesToCleanup filesToCleanup)
        {
        	int count = 0;
        	bool needsAssetDatabaseRefresh = false;
        	if (filesToCleanup.FilesToDelete.Count > 0)
        	{
        		needsAssetDatabaseRefresh = true;
    
				BuildLog.Info("PostBuildDelete removing {0} files total.", count);
        		foreach (string path in filesToCleanup.FilesToDelete)
        		{
        			string srcFile = Path.Combine(Application.dataPath, path);
        			BuildLog.Info("Deleting file {0} in PostBuildDelete", srcFile);
                    if (File.Exists(srcFile))
        			{
        				FileUtil.DeleteFileOrDirectory(srcFile);
        				count++;
        			}
                }
                BuildLog.Info("PostBuildDelete removed {0} files total.", count);
        	}
    
        	if (needsAssetDatabaseRefresh)
        	{
        		AssetDatabase.Refresh();
        	}
        }
        
        private void ValidateResults(BuildReport buildReport, TimeSpan delta)
        {
	        // Check build results
	        string? buildError = null;
	        if (buildReport.summary.result != BuildResult.Succeeded)
	        {
		        
		        
		        switch (buildReport.summary.result)
		        {
			        case BuildResult.Cancelled:
				        buildError = "Build cancelled";
				        break;

			        case BuildResult.Failed:
				        buildError = "Build failed";
				        break;

			        case BuildResult.Unknown:
				        buildError = "Build failed.  Ended in 'unknown' state";
				        break;

			        default:
				        buildError = "Default bad build result";
				        break;
		        }
	        }

	        string buildTime = string.Format("{0:00}:{1:00}:{2:00}", delta.Hours, delta.Minutes, delta.Seconds);
	        if (!string.IsNullOrEmpty(buildError))
	        {
		        BuildLog.Info("Build Succeeded! Time to Build: {0}", buildTime);
	        }
	        else
	        {
		        string message = string.Format("Building Client Failed: (Total Errors: {0}), Details: {1}",
			        buildReport.summary.totalErrors, buildError);
		        BuildLog.Error(message);
		        throw new BuildFailedException(string.Format("Build Failed! Message: {0} Time to failure: {1}",
			        buildError, buildTime));
	        } 
        }
    }
}
#nullable disable