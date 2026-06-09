#nullable enable
using UnityEngine;

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

        public Vector3? WorldPosition { get; init; }
        public float SpatialBlend { get; init; }
        public float MinDistance { get; init; }
        public float MaxDistance { get; init; }
        public bool Spatialize { get; init; }

        public StealPolicy StealPolicy { get; init; }
        public float StealFadeDuration { get; init; }
        public float Cooldown { get; init; }

        public static SoundParams Default => new()
        {
            Group = default,
            Volume = 1f,
            Pan = 0f,
            Loop = false,
            FadeInDuration = 0f,
            CrossfadeDuration = 0f,
            Ducking = null,
            MaxInstances = 0,
            WorldPosition = null,
            SpatialBlend = 0f,
            MinDistance = 1f,
            MaxDistance = 40f,
            Spatialize = false,
            StealPolicy = StealPolicy.StealOldest,
            StealFadeDuration = 0.08f,
            Cooldown = 0f
        };
    }
}
#nullable disable
