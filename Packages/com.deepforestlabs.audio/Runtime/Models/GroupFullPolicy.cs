#nullable enable
namespace DeepForestLabs.Audio
{
    /// <summary>
    /// Controls what happens when a play request would exceed a group's max voices.
    /// </summary>
    public enum GroupFullPolicy
    {
        /// <summary>Drop the new play request and let the group's voices finish naturally.</summary>
        RejectNew = 0,

        /// <summary>Immediately stop the group's oldest voice and play the new sound in its place.</summary>
        StealOldest = 1,
    }
}
#nullable disable
