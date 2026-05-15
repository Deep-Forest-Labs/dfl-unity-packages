#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.Audio.Editor
{
    [CustomEditor(typeof(SoundCatalog))]
    public sealed class SoundCatalogEditor : UnityEditor.Editor
    {
        private SerializedProperty _entries = null!;
        private readonly Dictionary<string, bool> _groupFoldouts = new();
        private readonly HashSet<int> _expandedRows = new();
        private static AudioSource? _previewSource;

        private void OnEnable()
        {
            _entries = serializedObject.FindProperty("_entries");
        }

        private void OnDisable()
        {
            StopPreview();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Sound Catalog", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawValidation();
            EditorGUILayout.Space();

            DrawBulkActions();
            EditorGUILayout.Space();

            DrawGroupedEntries();

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Entry"))
            {
                _entries.InsertArrayElementAtIndex(_entries.arraySize);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidation()
        {
            HashSet<string> keys = new();
            List<string> duplicates = new();
            List<int> emptyKeys = new();
            List<int> missingClips = new();

            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                string key = entry.FindPropertyRelative("_key").stringValue;

                if (string.IsNullOrEmpty(key))
                    emptyKeys.Add(i);
                else if (!keys.Add(key))
                    duplicates.Add(key);

                SerializedProperty clip = entry.FindPropertyRelative("_clip");
                SerializedProperty mode = clip.FindPropertyRelative("_mode");
                SerializedProperty guid = clip.FindPropertyRelative("_guid");
                SerializedProperty resPath = clip.FindPropertyRelative("_resourcesPath");
                if (string.IsNullOrEmpty(guid?.stringValue) && string.IsNullOrEmpty(resPath?.stringValue))
                    missingClips.Add(i);
            }

            if (duplicates.Count > 0)
                EditorGUILayout.HelpBox($"Duplicate keys: {string.Join(", ", duplicates)}", MessageType.Error);
            if (emptyKeys.Count > 0)
                EditorGUILayout.HelpBox($"Empty keys at indices: {string.Join(", ", emptyKeys)}", MessageType.Warning);
            if (missingClips.Count > 0)
                EditorGUILayout.HelpBox($"Missing clip refs at indices: {string.Join(", ", missingClips)}", MessageType.Error);
        }

        private void DrawBulkActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set All to Preload", EditorStyles.miniButtonLeft))
            {
                for (int i = 0; i < _entries.arraySize; i++)
                    _entries.GetArrayElementAtIndex(i).FindPropertyRelative("_preload").boolValue = true;
            }
            if (GUILayout.Button("Clear All Preload", EditorStyles.miniButtonMid))
            {
                for (int i = 0; i < _entries.arraySize; i++)
                    _entries.GetArrayElementAtIndex(i).FindPropertyRelative("_preload").boolValue = false;
            }
            if (GUILayout.Button("Clear All Ducking", EditorStyles.miniButtonRight))
            {
                for (int i = 0; i < _entries.arraySize; i++)
                    _entries.GetArrayElementAtIndex(i).FindPropertyRelative("_ducking").objectReferenceValue = null;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGroupedEntries()
        {
            Dictionary<string, List<int>> groups = new();

            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                SerializedProperty groupProp = entry.FindPropertyRelative("_group");
                SerializedProperty nameProp = groupProp?.FindPropertyRelative("_name");
                string groupName = nameProp?.stringValue;
                if (string.IsNullOrEmpty(groupName))
                    groupName = "(None)";

                if (!groups.ContainsKey(groupName))
                    groups[groupName] = new List<int>();
                groups[groupName].Add(i);
            }

            foreach (KeyValuePair<string, List<int>> kvp in groups.OrderBy(g => g.Key))
            {
                string groupName = kvp.Key;
                List<int> indices = kvp.Value;

                if (!_groupFoldouts.ContainsKey(groupName))
                    _groupFoldouts[groupName] = true;

                _groupFoldouts[groupName] = EditorGUILayout.Foldout(
                    _groupFoldouts[groupName],
                    $"{groupName} ({indices.Count} entries)",
                    true,
                    EditorStyles.foldoutHeader);

                if (!_groupFoldouts[groupName])
                    continue;

                EditorGUI.indentLevel++;
                for (int idx = 0; idx < indices.Count; idx++)
                {
                    int i = indices[idx];
                    DrawEntryRow(i);
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawEntryRow(int index)
        {
            SerializedProperty entry = _entries.GetArrayElementAtIndex(index);
            SerializedProperty keyProp = entry.FindPropertyRelative("_key");
            SerializedProperty clipProp = entry.FindPropertyRelative("_clip");
            SerializedProperty volumeProp = entry.FindPropertyRelative("_defaultVolume");
            SerializedProperty panProp = entry.FindPropertyRelative("_defaultPan");
            SerializedProperty maxInstProp = entry.FindPropertyRelative("_maxInstances");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(keyProp, GUIContent.none, GUILayout.Width(120));
            EditorGUILayout.PropertyField(clipProp, GUIContent.none, GUILayout.MinWidth(100));

            EditorGUILayout.LabelField("Vol", GUILayout.Width(24));
            volumeProp.floatValue = EditorGUILayout.Slider(volumeProp.floatValue, 0f, 1f, GUILayout.Width(100));

            EditorGUILayout.LabelField("Pan", GUILayout.Width(24));
            panProp.floatValue = EditorGUILayout.Slider(panProp.floatValue, -1f, 1f, GUILayout.Width(100));

            EditorGUILayout.LabelField("Max", GUILayout.Width(28));
            maxInstProp.intValue = EditorGUILayout.IntField(maxInstProp.intValue, GUILayout.Width(30));

            bool isExpanded = _expandedRows.Contains(index);
            if (GUILayout.Button(isExpanded ? "▼" : "►", GUILayout.Width(24)))
            {
                if (isExpanded) _expandedRows.Remove(index);
                else _expandedRows.Add(index);
            }

            if (GUILayout.Button("▶", GUILayout.Width(24)))
            {
                TestPlayEntry(entry);
            }

            bool deleted = GUILayout.Button("×", GUILayout.Width(20));

            EditorGUILayout.EndHorizontal();

            if (deleted)
            {
                _entries.DeleteArrayElementAtIndex(index);
                return;
            }

            if (_expandedRows.Contains(index))
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_group"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_poolPrewarm"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_preload"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("_ducking"));
                EditorGUI.indentLevel -= 2;
            }
        }

        private static void TestPlayEntry(SerializedProperty entry)
        {
            StopPreview();

            SerializedProperty clipProp = entry.FindPropertyRelative("_clip");
            SerializedProperty resPath = clipProp.FindPropertyRelative("_resourcesPath");
            SerializedProperty guid = clipProp.FindPropertyRelative("_guid");

            AudioClip? clip = null;

            if (!string.IsNullOrEmpty(guid?.stringValue))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid.stringValue);
                if (!string.IsNullOrEmpty(assetPath))
                    clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            }

            if (clip == null && !string.IsNullOrEmpty(resPath?.stringValue))
            {
                clip = Resources.Load<AudioClip>(resPath.stringValue);
            }

            if (clip == null)
            {
                Debug.LogWarning("Cannot preview: no clip found for this entry.");
                return;
            }

            GameObject go = EditorUtility.CreateGameObjectWithHideFlags(
                "AudioPreview", HideFlags.HideAndDontSave);
            _previewSource = go.AddComponent<AudioSource>();

            float vol = entry.FindPropertyRelative("_defaultVolume").floatValue;
            float pan = entry.FindPropertyRelative("_defaultPan").floatValue;

            _previewSource.clip = clip;
            _previewSource.volume = vol;
            _previewSource.panStereo = pan;
            _previewSource.Play();

            EditorApplication.update += CheckPreviewComplete;
        }

        private static void CheckPreviewComplete()
        {
            if (_previewSource == null || !_previewSource.isPlaying)
            {
                StopPreview();
                EditorApplication.update -= CheckPreviewComplete;
            }
        }

        private static void StopPreview()
        {
            if (_previewSource != null)
            {
                _previewSource.Stop();
                Object.DestroyImmediate(_previewSource.gameObject);
                _previewSource = null;
            }
        }
    }
}
#nullable disable
