#nullable enable
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Sentry;
using Sentry.Unity;
using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SentryOptionsConfiguration",
    menuName = "Sentry/SentryOptionsConfiguration",
    order = 999)]
public sealed class DflSentryOptionsConfiguration : SentryOptionsConfiguration
{
    public override void Configure(SentryUnityOptions options)
    {
        options.Release = BuildSettings.Instance.ShortVersion;

        options.Environment =
            BuildSettings.Instance.Environment.Name == "prod"
                ? "production"
                : BuildSettings.Instance.Environment.Name;

        options.Debug = false;
        options.EnableLogs = false;

        options.AddBreadcrumbsForLogType[LogType.Log] = false;
        options.AddBreadcrumbsForLogType[LogType.Assert] = false;
        options.AddBreadcrumbsForLogType[LogType.Warning] = true;
        options.AddBreadcrumbsForLogType[LogType.Error] = true;
        options.AddBreadcrumbsForLogType[LogType.Exception] = true;

        options.SetBeforeBreadcrumb(BeforeBreadcrumb);

        Log.Debug($"[Sentry] DflSentryOptionsConfiguration.Configure - Sentry.IsEnabled : {SentrySdk.IsEnabled}");
    }

    private static Breadcrumb? BeforeBreadcrumb(Breadcrumb breadcrumb)
    {
        // prevent infinite loop of setting breadcrumbs on breadcrumbs
        // this only applies if you turn on EnableLogs, but still a good prevention check since:
        // - there's no allocation or GC
        // - processing is very minimal
        // - we only add breadcrumbs for warning-tier and above (limited in production)
        if (!string.IsNullOrEmpty(breadcrumb.Message) &&
            breadcrumb.Message.StartsWith("Sentry:", StringComparison.Ordinal))
        {
            return null;
        }

        return breadcrumb.Category switch
        {
            "network.event" => null,
            "ui.lifecycle" => null,
            _ => breadcrumb
        };
    }
}
#nullable disable
