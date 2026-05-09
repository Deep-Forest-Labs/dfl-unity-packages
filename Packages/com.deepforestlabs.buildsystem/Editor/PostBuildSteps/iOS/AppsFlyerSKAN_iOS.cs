#if UNITY_IOS

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using DeepForestLabs.BuildSystems.PostBuildSteps;
using DeepForestLabs.Logger;

public class AppsFlyerSKAN_iOS : IPostprocessBuildWithReport
{
	public int callbackOrder => (int)PostBuildOrder.AppFlyerSkan;

	public void OnPostprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.iOS)
		{
			string plistPath = report.summary.outputPath + "/Info.plist";
			BuildLog.Info("AppsFlyerSKAN - Postprocessing \"{0}\" for target iOS", plistPath);

			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			PlistElementDict rootDict = plist.root;
			rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");

			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif
