#if UNITY_ANDROID
#nullable enable
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
    public class SetKeystoreInfo_Android : IPreprocessBuildWithReport
    {
        public int callbackOrder => (int)PreBuildOrder.SetKeystoreInfo;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            if (!SigningEnvironment.HasAndroidKeystoreConfig())
            {
                if (Application.isBatchMode)
                {
                    SigningEnvironment.RequireAndroidKeystoreForStoreBuild();
                }

                BuildLog.Info(
                    "PreBuild:SetKeystoreInfo - Skipping; Android keystore env vars not set (local/editor build).");
                return;
            }

            SigningEnvironment.RequireAndroidKeystoreForStoreBuild();

            string keystorePath = SigningEnvironment.Get(SigningEnvironment.AndroidKeystorePath)!;
            string keystorePass = SigningEnvironment.GetPassword(
                SigningEnvironment.AndroidKeystorePass,
                SigningEnvironment.AndroidKeystorePassB64)!;
            string alias = SigningEnvironment.Get(SigningEnvironment.AndroidKeyAlias)!;
            string aliasPass = SigningEnvironment.GetPassword(
                SigningEnvironment.AndroidKeyAliasPass,
                SigningEnvironment.AndroidKeyAliasPassB64)!;

            BuildLog.Info("PreBuild:SetKeystoreInfo - Applying keystore from env for target {0}", report.summary.platform);

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = aliasPass;
        }
    }
}
#nullable disable
#endif
