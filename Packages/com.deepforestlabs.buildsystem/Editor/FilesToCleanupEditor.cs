#nullable enable
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
    [CustomEditor(typeof(FilesToCleanup))]
    public sealed class FilesToCleanupEditor : Editor
    {
        private SerializedProperty _filesToBackupAndRestore = null!;
        private SerializedProperty _filesToDelete = null!;

        private void OnEnable()
        {
            _filesToBackupAndRestore = serializedObject.FindProperty("_filesToBackupAndRestore");
            _filesToDelete = serializedObject.FindProperty("_filesToDelete");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Files To Cleanup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawPathList("Files to Backup and Restore", _filesToBackupAndRestore);
            EditorGUILayout.Space();
            DrawPathList("Files to Delete", _filesToDelete);

            HandleDragAndDrop();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPathList(string label, SerializedProperty listProp)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                string path = elem.stringValue;
                bool exists = !string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path));

                EditorGUILayout.BeginHorizontal();

                Color prevColor = GUI.color;
                if (!exists && !string.IsNullOrEmpty(path))
                    GUI.color = new Color(1f, 0.6f, 0.6f);

                EditorGUILayout.PropertyField(elem, GUIContent.none);
                GUI.color = prevColor;

                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string selected = EditorUtility.OpenFilePanel("Select File", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(selected))
                    {
                        string projectPath = Path.GetFullPath(Application.dataPath + "/..");
                        if (selected.StartsWith(projectPath))
                            selected = selected.Substring(projectPath.Length + 1);
                        elem.stringValue = selected;
                    }
                }

                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (!exists && !string.IsNullOrEmpty(path))
                {
                    EditorGUILayout.HelpBox($"Path not found: {path}", MessageType.Warning);
                }
            }

            if (GUILayout.Button($"Add to {label}"))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
                listProp.GetArrayElementAtIndex(listProp.arraySize - 1).stringValue = string.Empty;
            }
        }

        private void HandleDragAndDrop()
        {
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0f, 40f, GUILayout.ExpandWidth(true));

            GUIStyle dropStyle = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter };
            GUI.Box(dropArea, "Drag files/folders here to add", dropStyle);

            if (evt.type == EventType.DragUpdated && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.AcceptDrag();
                foreach (string path in DragAndDrop.paths)
                {
                    _filesToBackupAndRestore.InsertArrayElementAtIndex(_filesToBackupAndRestore.arraySize);
                    _filesToBackupAndRestore
                        .GetArrayElementAtIndex(_filesToBackupAndRestore.arraySize - 1)
                        .stringValue = path;
                }
                evt.Use();
            }
        }
    }
}
#nullable disable
