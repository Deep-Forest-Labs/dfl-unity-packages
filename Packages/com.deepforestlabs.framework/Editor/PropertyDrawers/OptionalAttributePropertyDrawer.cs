#nullable enable
using System;
using System.Reflection;
using DeepForestLabs.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(OptionalAttribute))]
    public sealed class OptionalAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.IsNullableReference())
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference && 
                    fieldInfo.GetCustomAttribute<SerializeReference>() != null)
                {
                    label.text += "?";
                    // if (Event.current.type == EventType.ContextClick && position.Contains(Event.current.mousePosition))
                    // {
                    //     ShowContextMenu(property);
                    //     Event.current.Use();
                    // }
                }
                else if (property.propertyType == SerializedPropertyType.ObjectReference && 
                         typeof(Object).IsAssignableFrom(fieldInfo.FieldType))
                {
                    label.text += "?";
                }
            }

            EditorGUI.PropertyField(position, property, label, true);
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    
        // private void ShowContextMenu(SerializedProperty property)
        // {
        //     GenericMenu menu = new GenericMenu();
        //     bool isNull = string.IsNullOrEmpty(property.managedReferenceFullTypename);
        //
        //     if (isNull)
        //     {
        //         menu.AddItem(new GUIContent("Add"), false, () =>
        //         {
        //             property.serializedObject.Update();
        //             property.managedReferenceValue = Activator.CreateInstance(fieldInfo.FieldType);
        //             property.serializedObject.ApplyModifiedProperties();
        //         });
        //     }
        //     else
        //     {
        //         menu.AddItem(new GUIContent("Clear"), false, () =>
        //         {
        //             property.serializedObject.Update();
        //             property.managedReferenceValue = null;
        //             property.serializedObject.ApplyModifiedProperties();
        //         });    
        //     }
        //
        //     menu.ShowAsContext();
        // }
    }
}