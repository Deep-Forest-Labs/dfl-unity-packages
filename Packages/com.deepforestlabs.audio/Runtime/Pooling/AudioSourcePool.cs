#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DeepForestLabs.Audio
{
    internal sealed class AudioSourcePool
    {
        private const int DefaultInitialCapacity = 8;
        private const int DefaultMaxCapacity = 64;

        private readonly List<PooledAudioSource> _available = new();
        private readonly List<PooledAudioSource> _active = new();
        private readonly Transform _root;
        private readonly int _maxCapacity;

        public int ActiveCount => _active.Count;
        public int AvailableCount => _available.Count;
        public int TotalCount => _active.Count + _available.Count;

        public AudioSourcePool(Transform root, int initialCapacity = DefaultInitialCapacity, int maxCapacity = DefaultMaxCapacity)
        {
            _root = root;
            _maxCapacity = maxCapacity;

            for (int i = 0; i < initialCapacity; i++)
            {
                _available.Add(CreateSource());
            }
        }

        public PooledAudioSource? Rent(AudioMixerGroup? group, AudioClip clip)
        {
            PooledAudioSource source;
            if (_available.Count > 0)
            {
                int lastIndex = _available.Count - 1;
                source = _available[lastIndex];
                _available.RemoveAt(lastIndex);
            }
            else if (TotalCount < _maxCapacity)
            {
                source = CreateSource();
            }
            else
            {
                return null;
            }

            source.IsActive = true;
            _active.Add(source);
            return source;
        }

        public void Return(PooledAudioSource source)
        {
            if (!_active.Remove(source)) return;
            source.Reset();
            _available.Add(source);
        }

        public void ReturnAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                PooledAudioSource source = _active[i];
                source.Reset();
                _available.Add(source);
            }
            _active.Clear();
        }

        public IReadOnlyList<PooledAudioSource> GetActiveSources() => _active;

        public void Prewarm(int count)
        {
            int toAdd = Mathf.Min(count, _maxCapacity - TotalCount);
            for (int i = 0; i < toAdd; i++)
            {
                _available.Add(CreateSource());
            }
        }

        private PooledAudioSource CreateSource()
        {
            return new PooledAudioSource(_root);
        }

        public int CountActiveInstances(AudioClip clip)
        {
            int count = 0;
            foreach (PooledAudioSource source in _active)
            {
                if (source.AssignedClip == clip) count++;
            }
            return count;
        }

        /// <summary>
        /// Returns the oldest active source playing the given clip, or null if none are active.
        /// The caller (AudioService) decides how to reclaim it so the associated handle stays consistent.
        /// </summary>
        public PooledAudioSource? FindStealCandidate(AudioClip clip)
        {
            foreach (PooledAudioSource source in _active)
            {
                if (source.AssignedClip == clip) return source;
            }
            return null;
        }
    }
}
#nullable disable
