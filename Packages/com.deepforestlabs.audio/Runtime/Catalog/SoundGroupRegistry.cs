#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    [CreateAssetMenu(fileName = "SoundGroupRegistry", menuName = "Audio/Sound Group Registry")]
    public sealed class SoundGroupRegistry : ScriptableObject, ISerializationCallbackReceiver
    {
        private static readonly string[] BuiltInGroups = { "BGM", "SFX", "UI" };

        [SerializeField] private List<string> _customGroups = new();

        private HashSet<string> _allGroups = new();

        public IReadOnlyCollection<string> AllGroups => _allGroups;

        public bool Contains(string groupName) => _allGroups.Contains(groupName);

        public string[] GetAllGroupNames()
        {
            string[] result = new string[_allGroups.Count];
            _allGroups.CopyTo(result);
            return result;
        }

        public void OnAfterDeserialize()
        {
            _allGroups = new HashSet<string>(BuiltInGroups);
            foreach (string custom in _customGroups)
            {
                if (!string.IsNullOrEmpty(custom))
                {
                    _allGroups.Add(custom);
                }
            }
        }

        public void OnBeforeSerialize() { }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = _customGroups.Count - 1; i >= 0; i--)
            {
                string g = _customGroups[i];
                if (string.IsNullOrWhiteSpace(g))
                    continue;

                foreach (string builtIn in BuiltInGroups)
                {
                    if (string.Equals(g, builtIn, System.StringComparison.OrdinalIgnoreCase))
                    {
                        _customGroups.RemoveAt(i);
                        break;
                    }
                }
            }
        }
#endif
    }
}
#nullable disable
