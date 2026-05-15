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
            SerializedProperty? nameProp = FindNameProperty(property);
            string current = nameProp?.stringValue ?? string.Empty;

            string[] groups = GetAvailableGroups();
            int selectedIndex = System.Array.IndexOf(groups, current);
            bool isCustom = selectedIndex < 0 && current.Length > 0;

            EditorGUI.BeginProperty(position, label, property);

            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            string[] displayOptions = new string[groups.Length + 1];
            System.Array.Copy(groups, displayOptions, groups.Length);
            displayOptions[groups.Length] = "Custom...";

            int displayIndex = isCustom ? groups.Length : (selectedIndex >= 0 ? selectedIndex : 0);

            int newIndex = EditorGUI.Popup(dropdownRect, label.text, displayIndex, displayOptions);

            if (newIndex != displayIndex && nameProp != null)
            {
                if (newIndex < groups.Length)
                    nameProp.stringValue = groups[newIndex];
                else
                    nameProp.stringValue = string.Empty;
            }

            bool showCustomField = newIndex >= groups.Length;
            if (showCustomField)
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
            SerializedProperty? nameProp = FindNameProperty(property);
            string current = nameProp?.stringValue ?? string.Empty;
            string[] groups = GetAvailableGroups();
            int selectedIndex = System.Array.IndexOf(groups, current);
            bool isCustom = selectedIndex < 0 && current.Length > 0;

            if (isCustom)
                return EditorGUIUtility.singleLineHeight * 2f + 2f;

            return EditorGUIUtility.singleLineHeight;
        }

        private static SerializedProperty? FindNameProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative("_name")
                ?? property.serializedObject.FindProperty(property.propertyPath + "._name");
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
