#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    [CreateAssetMenu(fileName = "SoundCatalog", menuName = "Audio/Sound Catalog")]
    public sealed class SoundCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SoundEntry> _entries = new();

        private Dictionary<string, SoundEntry> _map = new();

        public IReadOnlyList<SoundEntry> Entries => _entries;

        public bool TryGetEntry(string key, out SoundEntry entry) =>
            _map.TryGetValue(key, out entry!);

        public void OnAfterDeserialize()
        {
            _map = new Dictionary<string, SoundEntry>();
            foreach (SoundEntry entry in _entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    _map[entry.Key] = entry;
                }
            }
        }

        public void OnBeforeSerialize() { }
    }
}
#nullable disable
