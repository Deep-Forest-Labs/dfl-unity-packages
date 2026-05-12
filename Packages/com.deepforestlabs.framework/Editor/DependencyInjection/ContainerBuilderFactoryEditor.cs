#nullable enable
using System.Collections.Generic;
using DeepForestLabs.Factories;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.EditorTools
{
    [CustomEditor(typeof(ContainerBuilderFactory), true)]
    [CanEditMultipleObjects]
    public class ContainerBuilderFactoryEditor : UnityEditor.Editor
    {
        private bool _showServiceSummary = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawServiceSummary();
            EditorGUILayout.Space();

            DrawUnassignedWarnings();
            EditorGUILayout.Space();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawServiceSummary()
        {
            _showServiceSummary = EditorGUILayout.Foldout(_showServiceSummary, "Registered Services", true, EditorStyles.foldoutHeader);
            if (!_showServiceSummary) return;

            EditorGUI.indentLevel++;

            List<string> services = new();

            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script") continue;

                    string typeName = iterator.type;
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        Object? obj = iterator.objectReferenceValue;
                        string label = obj != null
                            ? $"{iterator.displayName}: {obj.GetType().Name} ({obj.name})"
                            : $"{iterator.displayName}: (unassigned)";
                        services.Add(label);
                    }
                    else if (typeName.Contains("AssetRef"))
                    {
                        services.Add($"{iterator.displayName} [{typeName}]");
                    }
                } while (iterator.NextVisible(false));
            }

            if (services.Count == 0)
            {
                EditorGUILayout.LabelField("No serialized service references found.", EditorStyles.miniLabel);
            }
            else
            {
                foreach (string service in services)
                {
                    EditorGUILayout.LabelField(service, EditorStyles.miniLabel);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawUnassignedWarnings()
        {
            List<string> unassigned = new();

            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script") continue;

                    if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                        iterator.objectReferenceValue == null)
                    {
                        unassigned.Add(iterator.displayName);
                    }
                } while (iterator.NextVisible(false));
            }

            if (unassigned.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    $"Unassigned references: {string.Join(", ", unassigned)}",
                    MessageType.Warning);
            }
        }
    }
}
#nullable disable
