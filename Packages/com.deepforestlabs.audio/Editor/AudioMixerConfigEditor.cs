#nullable enable
using System.Linq;
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
        private SerializedProperty _sizePoolToRealVoices = null!;

        private void OnEnable()
        {
            _mixer = serializedObject.FindProperty("_mixer");
            _masterVolumeParam = serializedObject.FindProperty("_masterVolumeParam");
            _groupMappings = serializedObject.FindProperty("_groupMappings");
            _sizePoolToRealVoices = serializedObject.FindProperty("_sizePoolToRealVoices");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_mixer);
            EditorGUILayout.PropertyField(_masterVolumeParam);
            EditorGUILayout.PropertyField(_sizePoolToRealVoices, new GUIContent("Size Pool To Real Voices",
                "Size the AudioSource pool to Project Settings > Audio > Max Real Voices instead of 64."));

            DrawValidation();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Group Mappings", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto-Map Groups from Mixer"))
            {
                AutoMapGroups();
            }

            EditorGUILayout.Space();
            DrawGroupMappings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGroupMappings()
        {
            _groupMappings.isExpanded = EditorGUILayout.Foldout(_groupMappings.isExpanded,
                $"Group Mappings ({_groupMappings.arraySize})", true, EditorStyles.foldoutHeader);

            if (!_groupMappings.isExpanded) return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < _groupMappings.arraySize; i++)
            {
                SerializedProperty mapping = _groupMappings.GetArrayElementAtIndex(i);
                SerializedProperty groupName = mapping.FindPropertyRelative("_groupName");

                string label = string.IsNullOrEmpty(groupName.stringValue)
                    ? $"Element {i}"
                    : groupName.stringValue;

                mapping.isExpanded = EditorGUILayout.Foldout(mapping.isExpanded, label, true);
                if (!mapping.isExpanded) continue;

                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(groupName, new GUIContent("Group Name"));
                EditorGUILayout.PropertyField(mapping.FindPropertyRelative("_mixerGroup"),
                    new GUIContent("Mixer Group"));
                EditorGUILayout.PropertyField(mapping.FindPropertyRelative("_volumeParam"),
                    new GUIContent("Volume Param"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Voice Budget", EditorStyles.miniLabel);

                SerializedProperty maxVoices = mapping.FindPropertyRelative("_maxVoices");
                SerializedProperty reservedVoices = mapping.FindPropertyRelative("_reservedVoices");

                EditorGUILayout.PropertyField(maxVoices, new GUIContent("Max Voices",
                    "Maximum concurrent voices for this group. 0 = no limit."));
                EditorGUILayout.PropertyField(reservedVoices, new GUIContent("Reserved Voices",
                    "Pool slots reserved exclusively for this group."));
                EditorGUILayout.PropertyField(mapping.FindPropertyRelative("_whenFull"), new GUIContent("When Full",
                    "What a new sound does when the group is at Max Voices."));
                EditorGUILayout.PropertyField(mapping.FindPropertyRelative("_priority"), new GUIContent("Priority",
                    "AudioSource priority: 0 = most important, 256 = least. Catalog entries can override it."));

                if (maxVoices.intValue < 0) maxVoices.intValue = 0;
                if (reservedVoices.intValue < 0) reservedVoices.intValue = 0;
                if (maxVoices.intValue > 0 && reservedVoices.intValue > maxVoices.intValue)
                {
                    EditorGUILayout.HelpBox(
                        "Reserved voices exceeds max voices — reserved will be clamped to max at runtime.",
                        MessageType.Warning);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                bool wasEmpty = _groupMappings.arraySize == 0;
                _groupMappings.InsertArrayElementAtIndex(_groupMappings.arraySize);
                if (wasEmpty)
                {
                    SetNewMappingDefaults(_groupMappings.GetArrayElementAtIndex(0));
                }
            }
            using (new EditorGUI.DisabledGroupScope(_groupMappings.arraySize == 0))
            {
                if (GUILayout.Button("-", GUILayout.Width(24)) && _groupMappings.arraySize > 0)
                {
                    _groupMappings.DeleteArrayElementAtIndex(_groupMappings.arraySize - 1);
                }
            }
            EditorGUILayout.EndHorizontal();

            DrawReservationSummary();

            EditorGUI.indentLevel--;
        }

        private void DrawReservationSummary()
        {
            int totalReserved = 0;
            int totalMaxVoices = 0;
            bool anyUncapped = false;
            for (int i = 0; i < _groupMappings.arraySize; i++)
            {
                SerializedProperty mapping = _groupMappings.GetArrayElementAtIndex(i);
                totalReserved += mapping.FindPropertyRelative("_reservedVoices").intValue;
                int maxVoices = mapping.FindPropertyRelative("_maxVoices").intValue;
                totalMaxVoices += maxVoices;
                anyUncapped |= maxVoices <= 0;
            }

            int realVoices = AudioSettings.GetConfiguration().numRealVoices;
            int poolMax = AudioMixerConfig.ResolvePoolCapacity(_sizePoolToRealVoices.boolValue, realVoices);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                $"Voice budget: pool max {poolMax} ({realVoices} real voices). " +
                $"Groups: {totalMaxVoices} max voices, {totalReserved} reserved, {poolMax - totalReserved} unreserved.",
                MessageType.Info);

            if (poolMax > realVoices)
            {
                EditorGUILayout.HelpBox(
                    "The pool can outgrow Unity's real voices, and then Unity silences sounds by AudioSource priority. " +
                    "Enable Size Pool To Real Voices so the pool decides instead.",
                    MessageType.Warning);
            }

            if (anyUncapped || totalMaxVoices > poolMax)
            {
                EditorGUILayout.HelpBox(
                    "Group max voices can add up to more than the pool. When the pool is full, a new sound takes the " +
                    "voice of a less important sound (by priority) or is rejected.",
                    MessageType.Info);
            }

            if (totalReserved >= poolMax)
            {
                EditorGUILayout.HelpBox(
                    "Total reservations consume the entire pool — unreserved groups will be unable to play.",
                    MessageType.Error);
            }
        }

        private static void SetNewMappingDefaults(SerializedProperty mapping)
        {
            mapping.FindPropertyRelative("_whenFull").enumValueIndex = (int)GroupFullPolicy.RejectNew;
            mapping.FindPropertyRelative("_priority").intValue = PooledAudioSource.DefaultPriority;
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

                AudioMixerGroup? match = allGroups.FirstOrDefault(g =>
                    string.Equals(g.name, regGroup, System.StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    int newIndex = _groupMappings.arraySize;
                    _groupMappings.InsertArrayElementAtIndex(newIndex);
                    SerializedProperty newMapping = _groupMappings.GetArrayElementAtIndex(newIndex);
                    newMapping.FindPropertyRelative("_groupName").stringValue = regGroup;
                    newMapping.FindPropertyRelative("_mixerGroup").objectReferenceValue = match;
                    newMapping.FindPropertyRelative("_volumeParam").stringValue = $"{regGroup}Volume";
                    SetNewMappingDefaults(newMapping);
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
