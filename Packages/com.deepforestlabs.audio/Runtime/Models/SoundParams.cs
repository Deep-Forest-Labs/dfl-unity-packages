#nullable enable
namespace DeepForestLabs.Audio
{
    public readonly struct SoundParams
    {
        public SoundGroupId Group { get; init; }
        public float Volume { get; init; }
        public float Pan { get; init; }
        public bool Loop { get; init; }
        public float FadeInDuration { get; init; }
        public float CrossfadeDuration { get; init; }
        public DuckingProfile? Ducking { get; init; }
        public int MaxInstances { get; init; }

        public static SoundParams Default => new()
        {
            Group = default,
            Volume = 1f,
            Pan = 0f,
            Loop = false,
            FadeInDuration = 0f,
            CrossfadeDuration = 0f,
            Ducking = null,
            MaxInstances = 0
        };
    }
}
#nullable disable
