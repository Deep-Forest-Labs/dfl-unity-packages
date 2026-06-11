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

        private Dictionary<string, GroupMapping> _map = new();

        public AudioMixer Mixer => _mixer;
        public string MasterVolumeParam => _masterVolumeParam;
        public IReadOnlyList<GroupMapping> GroupMappings => _groupMappings;

        public bool TryGetMapping(SoundGroupId groupId, out GroupMapping mapping) =>
            _map.TryGetValue(groupId.Name, out mapping!);

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

            public string GroupName => _groupName;
            public AudioMixerGroup MixerGroup => _mixerGroup;
            public string VolumeParam => _volumeParam;
            public int MaxVoices => _maxVoices;
            public int ReservedVoices => _reservedVoices;
        }
    }
}
#nullable disable
