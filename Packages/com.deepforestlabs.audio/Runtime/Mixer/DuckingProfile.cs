#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    [CreateAssetMenu(fileName = "DuckingProfile", menuName = "Audio/Ducking Profile")]
    public sealed class DuckingProfile : ScriptableObject
    {
        [SerializeField] private SoundGroupId _targetGroup = SoundGroupId.Bgm;
        [Tooltip("More groups this profile ducks by the same amount, on the same attack and release.")]
        [SerializeField] private SoundGroupId[] _alsoDucks = Array.Empty<SoundGroupId>();
        [SerializeField] [Range(-80f, 0f)] private float _volumeReductionDb = -12f;
        [SerializeField] [Range(0f, 2f)] private float _attackTime = 0.1f;
        [SerializeField] [Range(0f, 5f)] private float _releaseTime = 0.5f;

        [NonSerialized] private SoundGroupId[]? _targetGroups;

        public SoundGroupId TargetGroup => _targetGroup;
        public float VolumeReductionDb => _volumeReductionDb;
        public float AttackTime => _attackTime;
        public float ReleaseTime => _releaseTime;

        /// <summary><see cref="TargetGroup"/> then the "also ducks" groups, without repeats or unnamed groups.</summary>
        public IReadOnlyList<SoundGroupId> TargetGroups => _targetGroups ??= ResolveTargetGroups();

        private SoundGroupId[] ResolveTargetGroups()
        {
            var groups = new List<SoundGroupId>(1 + _alsoDucks.Length);
            AddGroup(groups, _targetGroup);
            foreach (SoundGroupId group in _alsoDucks)
                AddGroup(groups, group);
            return groups.ToArray();
        }

        private static void AddGroup(List<SoundGroupId> groups, SoundGroupId group)
        {
            if (!string.IsNullOrEmpty(group.Name) && !groups.Contains(group))
                groups.Add(group);
        }

#if UNITY_EDITOR
        private void OnValidate() => _targetGroups = null;
#endif
    }
}
#nullable disable
