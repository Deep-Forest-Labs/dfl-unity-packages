#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.Audio.Editor
{
    [CustomPropertyDrawer(typeof(SoundGroupId))]
    public sealed class SoundGroupIdPropertyDrawer : PropertyDrawer
    {
        private static readonly string[] DefaultGroups = { "BGM", "SFX", "UI" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty? nameProp = property.FindPropertyRelative("_name");
            string current = nameProp?.stringValue ?? string.Empty;

            string[] groups = GetAvailableGroups();
            int selectedIndex = System.Array.IndexOf(groups, current);

            EditorGUI.BeginProperty(position, label, property);

            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            string[] displayOptions = new string[groups.Length + 1];
            System.Array.Copy(groups, displayOptions, groups.Length);
            displayOptions[groups.Length] = "Custom...";

            int displayIndex = selectedIndex >= 0 ? selectedIndex : groups.Length;

            int newIndex = EditorGUI.Popup(dropdownRect, label.text, displayIndex, displayOptions);

            if (nameProp != null && newIndex != displayIndex)
            {
                if (newIndex < groups.Length)
                {
                    nameProp.stringValue = groups[newIndex];
                }
            }

            if (newIndex >= groups.Length || selectedIndex < 0)
            {
                Rect textRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2f,
                    position.width, EditorGUIUtility.singleLineHeight);
                string value = EditorGUI.TextField(textRect, "Custom Name", current);
                if (nameProp != null)
                    nameProp.stringValue = value;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty? nameProp = property.FindPropertyRelative("_name");
            string current = nameProp?.stringValue ?? string.Empty;
            string[] groups = GetAvailableGroups();
            int selectedIndex = System.Array.IndexOf(groups, current);

            if (selectedIndex < 0)
            {
                return EditorGUIUtility.singleLineHeight * 2f + 2f;
            }

            return EditorGUIUtility.singleLineHeight;
        }

        private static string[] GetAvailableGroups()
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

            return DefaultGroups;
        }
    }
}
#nullable disable
