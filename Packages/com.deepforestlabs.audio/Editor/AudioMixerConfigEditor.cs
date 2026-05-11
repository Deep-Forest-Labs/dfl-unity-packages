#nullable enable
using UnityEditor;
using UnityEngine;

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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Group Mappings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_groupMappings, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#nullable disable
