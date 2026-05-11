#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.Audio.Editor
{
    [CustomEditor(typeof(SoundCatalog))]
    public sealed class SoundCatalogEditor : UnityEditor.Editor
    {
        private SerializedProperty _entries = null!;

        private void OnEnable()
        {
            _entries = serializedObject.FindProperty("_entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Sound Catalog", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            ValidateDuplicateKeys();

            EditorGUILayout.PropertyField(_entries, new GUIContent("Entries"), true);

            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateDuplicateKeys()
        {
            HashSet<string> keys = new();
            List<string> duplicates = new();

            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = entry.FindPropertyRelative("_key");
                string key = keyProp.stringValue;

                if (!string.IsNullOrEmpty(key) && !keys.Add(key))
                {
                    duplicates.Add(key);
                }
            }

            if (duplicates.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    $"Duplicate keys found: {string.Join(", ", duplicates)}",
                    MessageType.Error);
            }

            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                SerializedProperty clipProp = entry.FindPropertyRelative("_clip");
                SerializedProperty keyProp = entry.FindPropertyRelative("_key");

                if (string.IsNullOrEmpty(keyProp.stringValue))
                {
                    EditorGUILayout.HelpBox(
                        $"Entry at index {i} has an empty key.",
                        MessageType.Warning);
                }
            }
        }
    }
}
#nullable disable
