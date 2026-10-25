#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DeepForestLabs.Audio
{
    [CreateAssetMenu(fileName = "AudioMixerConfig", menuName = "Audio/Mixer Config")]
    public sealed class AudioMixerConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private AudioMixer _mixer = default!;
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private List<GroupMapping> _groupMappings = new();

        [Header("Voice Pool")]
        [Tooltip("Size the AudioSource pool to Project Settings > Audio > Max Real Voices instead of 64, " +
                 "so Unity never has to virtualize a pooled sound.")]
        [SerializeField] private bool _sizePoolToRealVoices;

        private Dictionary<string, GroupMapping> _map = new();

        public AudioMixer Mixer => _mixer;
        public string MasterVolumeParam => _masterVolumeParam;
        public IReadOnlyList<GroupMapping> GroupMappings => _groupMappings;
        public bool SizePoolToRealVoices => _sizePoolToRealVoices;

        public bool TryGetMapping(SoundGroupId groupId, out GroupMapping mapping) =>
            _map.TryGetValue(groupId.Name, out mapping!);

        public int ResolvePoolCapacity(int numRealVoices) =>
            ResolvePoolCapacity(_sizePoolToRealVoices, numRealVoices);

        internal static int ResolvePoolCapacity(bool sizePoolToRealVoices, int numRealVoices) =>
            sizePoolToRealVoices ? Mathf.Max(1, numRealVoices) : AudioSourcePool.DefaultMaxCapacity;

        public void OnAfterDeserialize()
        {
            _map = new Dictionary<string, GroupMapping>();
            foreach (GroupMapping mapping in _groupMappings)
            {
                if (!string.IsNullOrEmpty(mapping.GroupName))
                {
                    _map[mapping.GroupName] = mapping;
                }
            }
        }

        public void OnBeforeSerialize() { }

        [Serializable]
        public sealed class GroupMapping
        {
            [SerializeField] private string _groupName = string.Empty;
            [SerializeField] private AudioMixerGroup _mixerGroup = default!;
            [SerializeField] private string _volumeParam = string.Empty;

            [Header("Voice Budget")]
            [Tooltip("Maximum concurrent voices for this group. 0 = no limit (pool capacity still applies).")]
            [SerializeField] private int _maxVoices;
            [Tooltip("Voices reserved exclusively for this group. Other groups cannot consume these pool slots.")]
            [SerializeField] private int _reservedVoices;
            [Tooltip("What a new sound does when the group is at Max Voices.")]
            [SerializeField] private GroupFullPolicy _whenFull = GroupFullPolicy.RejectNew;
            [Tooltip("AudioSource priority for this group's sounds: 0 = most important, 256 = least. " +
                     "A catalog entry can override it. When the pool is full, a sound may take the voice " +
                     "of a less important one.")]
            [SerializeField] [Range(0, 256)] private int _priority = PooledAudioSource.DefaultPriority;

            public string GroupName => _groupName;
            public AudioMixerGroup MixerGroup => _mixerGroup;
            public string VolumeParam => _volumeParam;
            public int MaxVoices => _maxVoices;
            public int ReservedVoices => _reservedVoices;
            public GroupFullPolicy WhenFull => _whenFull;
            public int Priority => _priority;
        }
    }
}
#nullable disable
