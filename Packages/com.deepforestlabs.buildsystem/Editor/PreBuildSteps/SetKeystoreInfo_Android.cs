#if UNITY_ANDROID
using System;
using System.Text;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
	public class SetKeystoreInfo_Android : IPreprocessBuildWithReport
	{
		// TODO: Replace with your project's keystore credentials
		private const string KEYSTORE_NAME = "your-app.keystore";
		private const string KEYSTORE_PASS = "";
		private const string KEYSTORE_ALIAS_NAME = "your-alias";
		private const string KEYSTORE_ALIAS_PASS = "";

		public int callbackOrder => (int)PreBuildOrder.SetKeystoreInfo;

		public void OnPreprocessBuild(BuildReport report)
		{
			if (report.summary.platform == BuildTarget.Android)
			{
				BuildLog.Info("PreBuild:SetKeystoreInfo - Setting keystore info for target {0}", report.summary.platform);

				PlayerSettings.Android.keystoreName = KEYSTORE_NAME;
				PlayerSettings.Android.keystorePass = DecodeString(KEYSTORE_PASS);
				PlayerSettings.Android.keystorePass = PlayerSettings.Android.keystorePass;

				PlayerSettings.Android.keyaliasName = KEYSTORE_ALIAS_NAME;
				PlayerSettings.Android.keyaliasPass = DecodeString(KEYSTORE_ALIAS_PASS);
				PlayerSettings.Android.keyaliasPass = PlayerSettings.Android.keyaliasPass;
			}
		}

		private string DecodeString(string encodedString)
		{
			byte[] data = Convert.FromBase64String(encodedString);
			return Encoding.UTF8.GetString(data);
		}
	}
}
#endif
