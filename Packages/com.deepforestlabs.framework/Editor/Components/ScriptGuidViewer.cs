#nullable enable
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs
{
    public class ScriptGuidViewer : EditorWindow
    {
        [MenuItem("Tools/Show Script GUIDs")]
        public static void ShowWindow()
        {
            GetWindow<ScriptGuidViewer>("Script GUIDs");
        }
    
        private void OnGUI()
        {
            if (Selection.activeGameObject)
            {
                foreach (Component comp in Selection.activeGameObject.GetComponents<Component>())
                {
                    if (comp == null) continue;

                    MonoScript script = MonoScript.FromMonoBehaviour(comp as MonoBehaviour);
                    string guid = script != null
                        ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script))
                        : "<no script>";

                    EditorGUILayout.SelectableLabel(comp.GetType().Name + " - " + guid);
                }
            }
            else if (Selection.activeObject is ScriptableObject so)
            {
                MonoScript script = MonoScript.FromScriptableObject(so);
                string guid = script != null
                    ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script))
                    : "<no script>";
                
                EditorGUILayout.SelectableLabel(Selection.activeObject.GetType().Name + " - " + guid);
            }
        }
    }
}
