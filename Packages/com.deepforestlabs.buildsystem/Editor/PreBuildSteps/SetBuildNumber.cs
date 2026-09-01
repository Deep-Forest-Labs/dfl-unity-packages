using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// Set the build number X.Y.Z (build)
namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
	public class SetBuildNumber : IPreprocessBuildWithReport
	{
		public int callbackOrder => (int)PreBuildOrder.SetBuildNumber;

		public void OnPreprocessBuild(BuildReport report)
		{
			BuildSettings buildSettings = BuildSettings.Instance;
			int buildNumber = buildSettings.BuildNumber;
			// Local/Editor BuildSettings often leave _buildNumber at -1 ("unset").
			// Android Gradle rejects `versionCode -1` (Groovy parses it as minus()).
			if (buildNumber <= 0)
			{
				buildNumber = 1;
				BuildLog.Info(
					"PreBuild:SetBuildNumber - BuildSettings.BuildNumber was {0}; using {1} for target {2}",
					buildSettings.BuildNumber,
					buildNumber,
					report.summary.platform);
			}
			else
			{
				BuildLog.Info("PreBuild:SetBuildNumber - Setting build number to {0} for target {1}", buildNumber, report.summary.platform);
			}

			switch (report.summary.platform)
			{
				case BuildTarget.iOS:
					PlayerSettings.iOS.buildNumber = buildNumber.ToString();
					break;

				case BuildTarget.Android:
					PlayerSettings.Android.bundleVersionCode = buildNumber;
					break;

			case BuildTarget.StandaloneWindows64:
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneOSX:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.WebGL:
				// No dedicated build number field; value is in BuildSettings
				break;
			}
		}
	}
}
