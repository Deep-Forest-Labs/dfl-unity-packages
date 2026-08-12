#nullable enable
using System;
using DeepForestLabs;

namespace DeepForestLabs.Platform
{
    public static class ContainerExtensions
    {
        public static IContainerBuilder AddPlatformServices(
            this IContainerBuilder builder,
            PlatformServiceOptions options = PlatformServiceOptions.Null)
        {
            switch (options)
            {
                case PlatformServiceOptions.Null:
                    return AddNullPlatformServices(builder);
                case PlatformServiceOptions.Firebase:
                    return AddFirebasePlatformServices(builder);
                default:
                    throw new ArgumentOutOfRangeException(nameof(options), options, "Unsupported PlatformServiceOptions.");
            }
        }

        private static IContainerBuilder AddNullPlatformServices(IContainerBuilder builder)
        {
            return builder
                .AddScoped<NullAnalyticsUiHelpers>()
                .AddAlias<IAnalyticsErrorHelper, NullAnalyticsUiHelpers>()
                .AddAlias<IAnalyticsUIEventHelper, NullAnalyticsUiHelpers>()
                .AddScoped<IAnalyticsService, NullAnalyticsService>()
                .AddScoped<IRemoteConfigService, NullRemoteConfigService>()
                .AddScoped<IBootConfigClient, NullBootConfigClient>()
                .AddScoped<IAdService, NullAdService>()
                .AddScoped<IIapService, NullIapService>()
                .AddScoped<ICloudSaveService, NullCloudSaveService>()
                .AddScoped<IPushNotificationService, NullPushNotificationService>()
                .AddScoped<IAccountService, NullAccountService>()
                .AddScoped<IConsentService, NullConsentService>();
        }

        private static IContainerBuilder AddFirebasePlatformServices(IContainerBuilder builder)
        {
            return builder
                .AddScoped<NullAnalyticsUiHelpers>()
                .AddAlias<IAnalyticsErrorHelper, NullAnalyticsUiHelpers>()
                .AddScoped<IAnalyticsUIEventHelper, FirebaseAnalyticsUiEventHelper>()
                .AddScoped<IAnalyticsService, FirebaseAnalyticsService>()
                .AddScoped<IRemoteConfigService, FirebaseRemoteConfigService>()
                .AddScoped<IBootConfigClient, NullBootConfigClient>()
                .AddScoped<IAdService, NullAdService>()
                .AddScoped<IIapService, NullIapService>()
                .AddScoped<ICloudSaveService, NullCloudSaveService>()
                .AddScoped<IPushNotificationService, NullPushNotificationService>()
                .AddScoped<IAccountService, NullAccountService>()
                .AddScoped<IConsentService, AttConsentService>();
        }
    }
}
#nullable disable
