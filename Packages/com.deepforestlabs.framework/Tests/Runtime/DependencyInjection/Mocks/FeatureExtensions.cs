#nullable enable
namespace DeepForestLabs.DependencyInjection.Mocks
{
    internal static class FeatureExtensions
    {
        public static IContainerBuilder AddScopedPlatformFeature(this IContainerBuilder container)
        {
            return container.AddScoped<IFeature>(_ => CreatePlatformFeature());
        }

        private static IFeature CreatePlatformFeature()
        {
#if UNITY_ANDROID
            return new FeatureAndroid();
#else
            return new Feature();
#endif
        }

        public static IContainerBuilder AddScopedFeature(this IContainerBuilder container)
        {
            return container.AddScoped<IFeature, Feature>();
        }
        
        public static IContainerBuilder AddSingletonFeature(this IContainerBuilder container)
        {
            return container.AddSingleton((IFeature)new Feature());
        }
        
        public static IContainerBuilder AddTransientFeature(this IContainerBuilder container)
        {
            return container.AddTransient<IFeature, Feature>();
        }

        public static IContainerBuilder AddSingletonFeatureAndroid(this IContainerBuilder container)
        {
            return container.AddSingleton((IFeature)new FeatureAndroid());
        }
    }
}
#nullable disable