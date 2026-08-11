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
                .AddScoped<IAdService, NullAdService>()
                .AddScoped<IIapService, NullIapService>()
                .AddScoped<ICloudSaveService, NullCloudSaveService>()
                .AddScoped<IPushNotificationService, NullPushNotificationService>()
                .AddScoped<IAccountService, NullAccountService>()
                .AddScoped<IConsentService, NullConsentService>()
                .AddScoped<UnityServicesWrapper>()
                .AddScoped<AnalyticsServiceWrapper>();
        }
    }
}
#nullable disable
