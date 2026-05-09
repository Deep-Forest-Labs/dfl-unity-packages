 // #nullable enable
 // using UnityEditor;
 // using UnityEngine;
 //
 // namespace DeepForestLabs.Components
 // {
 //     [CustomPropertyDrawer(typeof(UIDefaultImageLayer))]
 //     internal sealed class UICompositeImageLayerDrawer : PropertyDrawer
 //     {
 //         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
 //         {
 //             SerializedProperty modeProp = property.FindPropertyRelative("_mode");
 //             SerializedProperty effectsProp = property.FindPropertyRelative("_effects");
 //             UICompositeImageRenderMode mode = (UICompositeImageRenderMode)modeProp.enumValueIndex;
 //             float effectsHeight = effectsProp != null && property.isExpanded ? EditorGUI.GetPropertyHeight(effectsProp, true) + 2f : 0f;
 //             switch (mode)
 //             {
 //                 case UICompositeImageRenderMode.Dynamic:
 //                     return property.isExpanded 
 //                         ? EditorGUIUtility.singleLineHeight * 5f + 10f
 //                         : EditorGUIUtility.singleLineHeight + 4f;
 //                 case UICompositeImageRenderMode.Baked:
 //                 case UICompositeImageRenderMode.BakedVertex:
 //                 default:
 //                     return property.isExpanded 
 //                         ? EditorGUIUtility.singleLineHeight * 5f + 10f + effectsHeight
 //                         : EditorGUIUtility.singleLineHeight + 4f;
 //             }
 //         }
 //
 //         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
 //         {
 //             EditorGUI.BeginProperty(position, label, property);
 //             SerializedProperty spriteProp = property.FindPropertyRelative("_sprite");
 //             SerializedProperty enabledProp = property.FindPropertyRelative("_enabled");
 //             SerializedProperty colorProp = property.FindPropertyRelative("_color");
 //             SerializedProperty fillProp = property.FindPropertyRelative("_fillCenter");
 //             Sprite? sprite = spriteProp.objectReferenceValue as Sprite;
 //
 //             Rect bgRect = new Rect(position.x, position.y, position.width, GetPropertyHeight(property, label));
 //             EditorGUI.DrawRect(bgRect, new Color(0, 0, 0, 0.05f));
 //
 //             // Foldout with sprite name
 //             Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
 //             property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GetLayerHeader(spriteProp), true);
 //
 //             if (!property.isExpanded)
 //             {
 //                 // Draw compact summary row (inline sprite, color, enabled)
 //                 float third = position.width / 3f;
 //                 float y = position.y + EditorGUIUtility.singleLineHeight + 2;
 //
 //                 EditorGUI.PropertyField(new Rect(position.x, y, third - 4, EditorGUIUtility.singleLineHeight), spriteProp, GUIContent.none);
 //                 EditorGUI.PropertyField(new Rect(position.x + third, y, third - 4, EditorGUIUtility.singleLineHeight), colorProp, GUIContent.none);
 //                 EditorGUI.PropertyField(new Rect(position.x + third * 2, y, third, EditorGUIUtility.singleLineHeight), enabledProp, GUIContent.none);
 //             }
 //             else
 //             {
 //                 // Expanded layout
 //                 float y = position.y + EditorGUIUtility.singleLineHeight + 4;
 //                 EditorGUI.indentLevel++;
 //
 //                 EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), spriteProp);
 //                 y += EditorGUIUtility.singleLineHeight + 2;
 //
 //                 EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), enabledProp);
 //                 y += EditorGUIUtility.singleLineHeight + 2;
 //
 //                 EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), colorProp);
 //                 y += EditorGUIUtility.singleLineHeight + 2;
 //
 //                 EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), fillProp);
 //
 //                 // Draw _effects array
 //                 SerializedProperty effectsProp = property.FindPropertyRelative("_effects");
 //                 if (effectsProp != null)
 //                 {
 //                     y += EditorGUIUtility.singleLineHeight + 2;
 //                     EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), effectsProp, new GUIContent("Effects"), true);
 //                 }
 //
 //                 EditorGUI.indentLevel--;
 //             }
 //
 //             EditorGUI.EndProperty();
 //         }
 //
 //         private static string GetLayerHeader(SerializedProperty spriteProp)
 //         {
 //             return spriteProp.objectReferenceValue != null
 //                 ? $"Core Settings  •  {spriteProp.objectReferenceValue.name}"
 //                 : "Core Settings  •  (none)";
 //         }
 //     }
 // }