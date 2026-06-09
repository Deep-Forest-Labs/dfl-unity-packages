#nullable enable
namespace DeepForestLabs.Audio
{
    /// <summary>
    /// Controls what happens when a play request would exceed a sound's max instance count.
    /// </summary>
    public enum StealPolicy
    {
        /// <summary>Immediately stop and reclaim the oldest instance. Best for player-driven
        /// repeats (weapon fire) where the newest sound matters most.</summary>
        StealOldest = 0,

        /// <summary>Fade the oldest instance out over its steal fade duration before reclaiming,
        /// avoiding a hard cut. Best for looping/continuous voices.</summary>
        SoftSteal = 1,

        /// <summary>Drop the new play request and let existing instances finish naturally.
        /// Best for dense one-shot bursts (enemy deaths) where cutoffs are jarring.</summary>
        RejectNew = 2,
    }
}
#nullable disable
