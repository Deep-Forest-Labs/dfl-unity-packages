#nullable enable
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UICompositeImage))]
    public sealed class UICompositeImageEditor : Editor
    {
        private SerializedProperty _raycastTarget = default!;
        private SerializedProperty _sprite = default!;
        private SerializedProperty _material = default!;
        private SerializedProperty _mode = default!;
        private SerializedProperty _borderWidth = default!;
        private SerializedProperty _falloffDistance = default!;
        private SerializedProperty _modifier = default!;
        private SerializedProperty _radii = default!;
        private SerializedProperty _layers = default!;
        private SerializedProperty _maskable = default!;
        private SerializedProperty _color = default!;
        private SerializedProperty _type = default!;

        private void OnEnable()
        {
            _raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            _sprite = serializedObject.FindProperty("m_Sprite");
            _material = serializedObject.FindProperty("m_Material");
            _mode = serializedObject.FindProperty("_mode");
            _borderWidth = serializedObject.FindProperty("_borderWidth");
            _falloffDistance = serializedObject.FindProperty("_falloffDistance");
            _modifier = serializedObject.FindProperty("_modifier");
            _radii = serializedObject.FindProperty("_radii");
            _layers = serializedObject.FindProperty("_layers");
            _maskable = serializedObject.FindProperty("m_Maskable");
            _color = serializedObject.FindProperty("m_Color");
            _type = serializedObject.FindProperty("m_Type");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!_mode.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(_mode);
            }
            if (!_type.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(_type);
            }
            if (!_raycastTarget.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(_raycastTarget);
            }
            if (!_maskable.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(_maskable);
            }

            switch ((UICompositeImageRenderMode)_mode.enumValueIndex)
            {
                case UICompositeImageRenderMode.Baked:
                case UICompositeImageRenderMode.BakedVertex:
                    if (_layers.arraySize < 1)
                    {
                        _layers.InsertArrayElementAtIndex(0);
                    }
                    SerializedProperty defaultLayerProp  =_layers.GetArrayElementAtIndex(0);
                    SerializedProperty defaultLayerColorProp = defaultLayerProp.FindPropertyRelative("_color");
                    SerializedProperty defaultLayerEnabledProp = defaultLayerProp.FindPropertyRelative("_enabled");
                    SerializedProperty defaultLayerFillCenterProp = defaultLayerProp.FindPropertyRelative("_fillCenter");
                    SerializedProperty defaultLayerSpriteProp = defaultLayerProp.FindPropertyRelative("_sprite");
                    SerializedProperty defaultLayerEffectsProp = defaultLayerProp.FindPropertyRelative("_effects");
                    if (!defaultLayerEnabledProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(defaultLayerEnabledProp);
                    }

                    if (!defaultLayerFillCenterProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(defaultLayerFillCenterProp);
                    }

                    Color originalColor = GUI.backgroundColor;
                    if (!defaultLayerSpriteProp.hasMultipleDifferentValues)
                    {
                        GUI.backgroundColor = defaultLayerSpriteProp.objectReferenceValue == null
                            ? Color.yellow
                            : originalColor;
                    }
                    EditorGUILayout.PropertyField(defaultLayerSpriteProp);
                    GUI.backgroundColor = originalColor;
                    
                    if (!defaultLayerColorProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(defaultLayerColorProp);
                    }

                    DrawRadiiFields(false);

                    if (!defaultLayerEffectsProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(defaultLayerEffectsProp);
                    }
                    break;
                
                case UICompositeImageRenderMode.Dynamic:
                    if (!_borderWidth.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(_borderWidth);
                    }
                    if (!_falloffDistance.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(_falloffDistance);
                    }
                    if (!_modifier.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(_modifier);
                    }
                    if (!_color.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(_color);
                    }
                    DrawRadiiFields(true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRadiiFields(bool editable)
        {
            // Only show corner radii fields when modifier is not Round
            UICompositeImageModifierType modifierType = (UICompositeImageModifierType)_modifier.enumValueIndex;
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Corner Radii", EditorStyles.boldLabel);
                
            SerializedProperty xProp = _radii.FindPropertyRelative("x");
            SerializedProperty yProp = _radii.FindPropertyRelative("y");
            SerializedProperty wProp = _radii.FindPropertyRelative("w");
            SerializedProperty zProp = _radii.FindPropertyRelative("z");
                
            GUI.enabled = editable && modifierType != UICompositeImageModifierType.Round;
            if (modifierType == UICompositeImageModifierType.Round)
            {
                RectTransform tx = ((target as MonoBehaviour)!.transform as RectTransform)!;

                float roundRadius = Mathf.Min(tx.sizeDelta.x / 2, tx.sizeDelta.y / 2);

                xProp.floatValue = roundRadius;
                yProp.floatValue = roundRadius;
                wProp.floatValue = roundRadius;
                zProp.floatValue = roundRadius;
            }
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (!xProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(xProp, new GUIContent("Upper Left"));
                    }
                    if (!yProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(yProp, new GUIContent("Upper Right"));
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (!wProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(wProp, new GUIContent("Lower Left"));
                    }
                    if (!zProp.hasMultipleDifferentValues)
                    {
                        EditorGUILayout.PropertyField(zProp, new GUIContent("Lower Right"));
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }
    }
}
