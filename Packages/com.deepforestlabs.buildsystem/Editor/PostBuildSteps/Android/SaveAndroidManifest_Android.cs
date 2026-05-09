#if UNITY_ANDROID
using System.IO;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PostBuildSteps.Android
{
	public class SaveAndroidManifest_Android : IPostprocessBuildWithReport
	{
		public int callbackOrder => (int)PostBuildOrder.SaveAndroidManifest;

		private void CopyFile(string srcPath, string dstPath)
		{
			BuildLog.Info("PostBuildSaveAndroidManifest - Copying {0} to {1}", srcPath, dstPath);

			if (File.Exists(srcPath))
			{
				File.Copy(srcPath, dstPath, true);
				BuildLog.Info("Copy Successful!");
			}
			else
			{
				BuildLog.Warning("Could not find {0}", srcPath);
			}
		}

		private void CopySingleManifestFile(string buildType, string buildDirectory)
		{
			string srcPath =
				string.Format(
					"Temp/gradleOut/launcher/build/intermediates/bundle_manifest/{0}/bundle-manifest/AndroidManifest.xml",
					buildType);
			string dstPath = string.Format("{0}/AndroidManifest.xml", buildDirectory);

			CopyFile(srcPath, dstPath);
		}

		private void CopySplitManifestFiles(string buildType, string buildDirectory)
		{
			string baseSrcDirectory =
				string.Format("Temp/gradleOut/launcher/build/intermediates/bundle_manifest/{0}/bundle-manifest",
					buildType);
			BuildSettings buildSettings = BuildSettings.Instance;
			string baseAPKName = BuilderUtils.AndroidAPKName(buildSettings.Environment.Name, buildSettings.BuildNumber, buildSettings.Addressables.UniqueId).Replace(".apk", string.Empty);

			string[] archNames = { "arm64-v8a", "armeabi-v7a" };
			foreach (string arch in archNames)
			{
				string srcPath = string.Format("{0}/{1}/AndroidManifest.xml", baseSrcDirectory, arch);
				string dstPath = string.Format("{0}/{1}_{2}_AndroidManifest.xml", buildDirectory, baseAPKName, arch);

				CopyFile(srcPath, dstPath);
			}
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			if (report.summary.platform == BuildTarget.Android)
			{
				bool devBuild = ( report.summary.options & BuildOptions.Development ) == BuildOptions.Development;
				string buildType = devBuild ? "debug" : "release";
				string buildDirectory = Path.GetDirectoryName(report.summary.outputPath);

				if (!EditorUserBuildSettings.buildAppBundle && PlayerSettings.Android.buildApkPerCpuArchitecture)
				{
					CopySplitManifestFiles(buildType, buildDirectory);
				}
				else
				{
					CopySingleManifestFile(buildType, buildDirectory);
				}
			}
		}
	}
}
#endif
