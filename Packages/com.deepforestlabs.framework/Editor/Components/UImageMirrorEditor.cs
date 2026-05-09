using UnityEditor;
using UnityEditor.UI;

namespace DeepForestLabs.Components
{
    [CustomEditor(typeof(UIImageMirror))]
    public class UImageMirrorEditor : ImageEditor
    {
        private SerializedProperty _horizontal;
        private SerializedProperty _vertical;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            _horizontal = serializedObject.FindProperty("_horizontal");
            _vertical = serializedObject.FindProperty("_vertical");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.PropertyField(_horizontal);
            EditorGUILayout.PropertyField(_vertical);

            serializedObject.ApplyModifiedProperties();
        }
    }
}