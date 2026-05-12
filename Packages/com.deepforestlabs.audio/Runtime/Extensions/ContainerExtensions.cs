#nullable enable
namespace DeepForestLabs.Audio
{
    public static class ContainerExtensions
    {
        public static IContainerBuilder AddAudioService(
            this IContainerBuilder builder,
            AudioMixerConfig config,
            SoundCatalog? catalog = null,
            SoundGroupRegistry? groupRegistry = null)
        {
            builder
                .AddSingleton(config)
                .AddScoped<IAudioMixerProvider, AudioMixerProvider>()
                .AddScoped<AudioAssetCache>()
                .AddScoped<DuckingController>()
                .AddScoped<IAudioService, AudioService>();

            if (catalog != null)
            {
                builder.AddSingleton(catalog);
            }

            if (groupRegistry != null)
            {
                builder.AddSingleton(groupRegistry);
            }

            return builder;
        }
    }
}
#nullable disable
