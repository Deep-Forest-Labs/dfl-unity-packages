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
			BuildLog.Info("PreBuild:SetBuildNumber - Setting build number to {0} for target {1}", buildSettings.BuildNumber, report.summary.platform);

			switch (report.summary.platform)
			{
				case BuildTarget.iOS:
					PlayerSettings.iOS.buildNumber = buildSettings.BuildNumber.ToString();
					break;

				case BuildTarget.Android:
					PlayerSettings.Android.bundleVersionCode = buildSettings.BuildNumber;
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
