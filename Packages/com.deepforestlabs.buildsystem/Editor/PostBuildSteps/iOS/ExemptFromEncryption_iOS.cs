#if UNITY_IOS

using System;
using System.IO;
using DeepForestLabs.BuildSystems.PostBuildSteps;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class ExemptFromEncryption_iOS : IPostprocessBuildWithReport
{
	public int callbackOrder => (int)PostBuildOrder.ExemptFromEncryption;

	public void OnPostprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.iOS)
		{
			string plistPath = report.summary.outputPath + "/Info.plist";
			BuildLog.Info("PostBuildExemptFromEncryption - Postprocessing \"{0}\" for target iOS", plistPath);

			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			PlistElementDict rootDict = plist.root;
			rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif
