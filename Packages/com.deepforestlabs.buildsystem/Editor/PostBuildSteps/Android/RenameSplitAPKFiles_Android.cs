#if UNITY_ANDROID
using System.IO;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PostBuildSteps.Android
{
	public class RenameSplitAPKFiles_Android : IPostprocessBuildWithReport
	{
		public int callbackOrder => (int)PostBuildOrder.RenameSplitAPKFiles;

		private void RenameFile(string srcPath, string dstPath)
		{
			BuildLog.Info("RenameSplitAPKFiles_Android - Renaming {0} to {1}", srcPath, dstPath);

			if (File.Exists(srcPath))
			{
				if (File.Exists(dstPath))
				{
					File.Delete(dstPath);
				}
				File.Move(srcPath, dstPath);
				BuildLog.Info("Rename Successful!");
			}
			else
			{
				BuildLog.Warning("Could not find {0}", srcPath);
			}
		}

		private void RenameSplitAPKFiles(string buildDirectory, string baseAPKName)
		{
			string[] archNames = { "arm64-v8a", "armeabi-v7a" };
			foreach (string arch in archNames)
			{
				string srcPath = string.Format("{0}/{1}.{2}.apk", buildDirectory, PlayerSettings.productName, arch);
				string dstPath = string.Format("{0}/{1}_{2}.apk", buildDirectory, baseAPKName, arch);

				RenameFile(srcPath, dstPath);
			}
		}

		private void RenameMappingFile(string buildDirectory, string baseAPKName)
		{
			string srcPath = string.Format("{0}/mapping.txt", buildDirectory);
			string dstPath = string.Format("{0}/{1}_mapping.txt", buildDirectory, baseAPKName);

			RenameFile(srcPath, dstPath);
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			if (report.summary.platform == BuildTarget.Android && !EditorUserBuildSettings.buildAppBundle && PlayerSettings.Android.buildApkPerCpuArchitecture)
			{
				string outputPath = Path.GetDirectoryName(report.summary.outputPath);
				BuildSettings buildSettings = BuildSettings.Instance;
				string baseAPKName = BuilderUtils.AndroidAPKName(buildSettings.Environment.Name,
					buildSettings.BuildNumber, buildSettings.Addressables.UniqueId).Replace(".apk", string.Empty);
				RenameSplitAPKFiles(outputPath, baseAPKName);
				RenameMappingFile(outputPath, baseAPKName);
			}
		}
	}
}
#endif
