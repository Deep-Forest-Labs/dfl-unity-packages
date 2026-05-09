#nullable enable
using DeepForestLabs.BuildSystems;
using Sentry.Unity;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(
	fileName = "SentryCliOptionsConfiguration",
	menuName = "Sentry/SentryCliOptionsConfiguration",
	order = 999)]
public sealed class DflSentryCliOptionsConfiguration : SentryCliOptionsConfiguration
{
	// TODO: Set your Sentry project slugs
	private const string PROD_PROJECT = "your-app-prod";
	private const string DEV_PROJECT = "your-app-dev";

	public override void Configure(SentryCliOptions cliOptions)
	{
		var isProd = BuildSettings.Instance.Environment.Name == "prod";

		cliOptions.Project = isProd ? PROD_PROJECT : DEV_PROJECT;

		cliOptions.UploadSymbols |= isProd;
		cliOptions.UploadDevelopmentSymbols |= isProd;
	}
}
#endif
#nullable disable
