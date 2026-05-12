#nullable enable
using ZLinq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace DeepForestLabs.Audio.Editor
{
    [CustomEditor(typeof(AudioMixerConfig))]
    public sealed class AudioMixerConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _mixer = null!;
        private SerializedProperty _masterVolumeParam = null!;
        private SerializedProperty _groupMappings = null!;

        private void OnEnable()
        {
            _mixer = serializedObject.FindProperty("_mixer");
            _masterVolumeParam = serializedObject.FindProperty("_masterVolumeParam");
            _groupMappings = serializedObject.FindProperty("_groupMappings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_mixer);
            EditorGUILayout.PropertyField(_masterVolumeParam);

            DrawValidation();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Group Mappings", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto-Map Groups from Mixer"))
            {
                AutoMapGroups();
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_groupMappings, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidation()
        {
            AudioMixer? mixer = _mixer.objectReferenceValue as AudioMixer;
            if (mixer == null)
            {
                EditorGUILayout.HelpBox("No AudioMixer assigned.", MessageType.Error);
                return;
            }

            string masterParam = _masterVolumeParam.stringValue;
            if (!string.IsNullOrEmpty(masterParam) && !mixer.GetFloat(masterParam, out _))
            {
                EditorGUILayout.HelpBox(
                    $"Exposed parameter '{masterParam}' not found on mixer. Make sure it's exposed.",
                    MessageType.Warning);
            }

            for (int i = 0; i < _groupMappings.arraySize; i++)
            {
                SerializedProperty mapping = _groupMappings.GetArrayElementAtIndex(i);
                string volumeParam = mapping.FindPropertyRelative("_volumeParam").stringValue;
                if (!string.IsNullOrEmpty(volumeParam) && !mixer.GetFloat(volumeParam, out _))
                {
                    string groupName = mapping.FindPropertyRelative("_groupName").stringValue;
                    EditorGUILayout.HelpBox(
                        $"Group '{groupName}': exposed parameter '{volumeParam}' not found on mixer.",
                        MessageType.Warning);
                }
            }

            string[] registryGroups = GetRegistryGroups();
            if (registryGroups.Length > 0)
            {
                for (int i = 0; i < registryGroups.Length; i++)
                {
                    bool found = false;
                    for (int j = 0; j < _groupMappings.arraySize; j++)
                    {
                        string mappedName = _groupMappings.GetArrayElementAtIndex(j)
                            .FindPropertyRelative("_groupName").stringValue;
                        if (mappedName == registryGroups[i])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        EditorGUILayout.HelpBox(
                            $"Registry group '{registryGroups[i]}' has no mapping.",
                            MessageType.Info);
                    }
                }
            }
        }

        private void AutoMapGroups()
        {
            AudioMixer? mixer = _mixer.objectReferenceValue as AudioMixer;
            if (mixer == null)
            {
                Debug.LogWarning("Cannot auto-map: no mixer assigned.");
                return;
            }

            AudioMixerGroup[] allGroups = mixer.FindMatchingGroups(string.Empty);
            string[] registryGroups = GetRegistryGroups();

            foreach (string regGroup in registryGroups)
            {
                bool alreadyMapped = false;
                for (int i = 0; i < _groupMappings.arraySize; i++)
                {
                    if (_groupMappings.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("_groupName").stringValue == regGroup)
                    {
                        alreadyMapped = true;
                        break;
                    }
                }
                if (alreadyMapped) continue;

                AudioMixerGroup? match = allGroups.AsValueEnumerable().FirstOrDefault(g =>
                    string.Equals(g.name, regGroup, System.StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    int newIndex = _groupMappings.arraySize;
                    _groupMappings.InsertArrayElementAtIndex(newIndex);
                    SerializedProperty newMapping = _groupMappings.GetArrayElementAtIndex(newIndex);
                    newMapping.FindPropertyRelative("_groupName").stringValue = regGroup;
                    newMapping.FindPropertyRelative("_mixerGroup").objectReferenceValue = match;
                    newMapping.FindPropertyRelative("_volumeParam").stringValue = $"{regGroup}Volume";
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static string[] GetRegistryGroups()
        {
            string[] guids = AssetDatabase.FindAssets("t:SoundGroupRegistry");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                SoundGroupRegistry? registry = AssetDatabase.LoadAssetAtPath<SoundGroupRegistry>(path);
                if (registry != null)
                {
                    registry.OnAfterDeserialize();
                    return registry.GetAllGroupNames();
                }
            }
            return new[] { "BGM", "SFX", "UI" };
        }
    }
}
#nullable disable
