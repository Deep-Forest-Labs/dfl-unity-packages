#nullable enable
using UnityEngine;

namespace DeepForestLabs.Audio
{
    internal static class AudioMixerUtils
    {
        private const float MinDb = -80f;
        private const float MaxDb = 0f;

        public static float LinearToDecibels(float linear)
        {
            linear = Mathf.Clamp01(linear);
            if (linear <= 0f) return MinDb;
            return Mathf.Clamp(20f * Mathf.Log10(linear), MinDb, MaxDb);
        }

        public static float DecibelsToLinear(float db)
        {
            db = Mathf.Clamp(db, MinDb, MaxDb);
            if (db <= MinDb) return 0f;
            return Mathf.Pow(10f, db / 20f);
        }
    }
}
#nullable disable
