#if UNITY_ANDROID
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
	public class SetBuildSubtarget_Android : IPreprocessBuildWithReport
	{
		public int callbackOrder => (int)PreBuildOrder.SetBuildSubTarget;

		public void OnPreprocessBuild(BuildReport report)
		{
			if (report.summary.platform == BuildTarget.Android)
			{
				// Set the Android Build SubTarget
				EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;
				//EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
				//EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.DXT;
				//EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.PVRTC;

				BuildLog.Info("PreBuild:SetBuildSubtarget - Setting AndroidBuildSubtarget to {0}", EditorUserBuildSettings.androidBuildSubtarget);
			}
		}
	}
}
#endif
