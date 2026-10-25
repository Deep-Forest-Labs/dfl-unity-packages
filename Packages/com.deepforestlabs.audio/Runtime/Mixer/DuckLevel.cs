#nullable enable
using System.Collections.Generic;

namespace DeepForestLabs.Audio
{
    /// <summary>Where one group's duck should go, and how long the fade there takes.</summary>
    internal readonly struct DuckLevel
    {
        public readonly float OffsetDb;
        public readonly float FadeSeconds;

        public DuckLevel(float offsetDb, float fadeSeconds)
        {
            OffsetDb = offsetDb;
            FadeSeconds = fadeSeconds;
        }

        /// <summary>
        /// The deepest active profile sets the level. Going deeper fades on the fastest active
        /// attack. Going shallower, or back to 0 dB, fades on <paramref name="releaseSeconds"/>.
        /// </summary>
        public static DuckLevel Resolve(IReadOnlyList<DuckingProfile> active, float currentOffsetDb, float releaseSeconds)
        {
            float offset = 0f;
            float attack = float.MaxValue;
            foreach (DuckingProfile profile in active)
            {
                if (profile.VolumeReductionDb < offset)
                    offset = profile.VolumeReductionDb;
                if (profile.AttackTime < attack)
                    attack = profile.AttackTime;
            }

            if (offset < currentOffsetDb)
                return new DuckLevel(offset, attack);
            return new DuckLevel(offset, offset > currentOffsetDb ? releaseSeconds : 0f);
        }
    }
}
#nullable disable
