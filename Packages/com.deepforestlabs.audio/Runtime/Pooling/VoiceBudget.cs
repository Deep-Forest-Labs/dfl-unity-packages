#nullable enable
using UnityEngine;

namespace DeepForestLabs.Audio
{
    /// <summary>
    /// Decides whether a group may take a pooled voice, and which playing voice gives way when it may not.
    /// AudioService reclaims the chosen voice so its handle stays consistent.
    /// </summary>
    internal sealed class VoiceBudget
    {
        private readonly AudioSourcePool _pool;
        private readonly AudioMixerConfig _config;

        public VoiceBudget(AudioSourcePool pool, AudioMixerConfig config)
        {
            _pool = pool;
            _config = config;
        }

        public int ResolvePriority(SoundGroupId group, int? soundPriority)
        {
            int priority = soundPriority
                ?? (_config.TryGetMapping(group, out AudioMixerConfig.GroupMapping mapping)
                    ? mapping.Priority
                    : PooledAudioSource.DefaultPriority);
            return Mathf.Clamp(priority, 0, 256);
        }

        public bool IsGroupFull(SoundGroupId group, out GroupFullPolicy whenFull)
        {
            whenFull = GroupFullPolicy.RejectNew;
            if (!_config.TryGetMapping(group, out AudioMixerConfig.GroupMapping mapping) || mapping.MaxVoices <= 0)
            {
                return false;
            }

            whenFull = mapping.WhenFull;
            return _pool.CountActiveForGroup(group) >= mapping.MaxVoices;
        }

        public bool HasFreeVoice(SoundGroupId group)
        {
            int totalAvailable = _pool.MaxCapacity - _pool.ActiveCount;
            if (totalAvailable <= 0) return false;

            int reservedForOthers = 0;
            foreach (AudioMixerConfig.GroupMapping other in _config.GroupMappings)
            {
                if (other.ReservedVoices <= 0) continue;
                SoundGroupId otherGroup = new SoundGroupId(other.GroupName);
                if (otherGroup == group) continue;

                int unfilled = other.ReservedVoices - _pool.CountActiveForGroup(otherGroup);
                if (unfilled > 0) reservedForOthers += unfilled;
            }

            return totalAvailable > reservedForOthers;
        }

        /// <summary>
        /// Returns the least important playing voice that is strictly less important than
        /// <paramref name="priority"/> (oldest first on ties), or null. Voices inside another group's
        /// reservation are skipped, since reclaiming them frees a slot only that group may use.
        /// </summary>
        public PooledAudioSource? FindPriorityStealCandidate(SoundGroupId group, int priority)
        {
            PooledAudioSource? candidate = null;
            foreach (PooledAudioSource source in _pool.GetActiveSources())
            {
                if (source.Priority <= priority) continue;
                if (candidate != null && source.Priority <= candidate.Priority) continue;
                if (source.Group != group && IsWithinReservation(source.Group)) continue;
                candidate = source;
            }
            return candidate;
        }

        private bool IsWithinReservation(SoundGroupId group)
        {
            return _config.TryGetMapping(group, out AudioMixerConfig.GroupMapping mapping)
                   && mapping.ReservedVoices > 0
                   && _pool.CountActiveForGroup(group) <= mapping.ReservedVoices;
        }
    }
}
#nullable disable
