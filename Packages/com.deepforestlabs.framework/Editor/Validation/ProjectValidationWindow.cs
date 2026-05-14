#nullable enable
using System.Collections.Generic;
using System.Linq;
using ZLinq;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.EditorTools.Validation
{
    public sealed class ProjectValidationWindow : EditorWindow
    {
        private List<ValidationResult> _results = new();
        private Vector2 _scrollPos;
        private readonly Dictionary<string, bool> _categoryFoldouts = new();

        [MenuItem("Deep Forest Labs/Tools/Validate Project")]
        public static void ShowWindow()
        {
            ProjectValidationWindow window = GetWindow<ProjectValidationWindow>("Project Validation");
            window.RunValidation();
            window.Show();
        }

        public void RunValidation()
        {
            _results = ProjectValidator.ValidateProject();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Re-Validate", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                RunValidation();
            }

            int errors = _results.AsValueEnumerable().Count(r => r.Severity == ValidationSeverity.Error);
            int warnings = _results.AsValueEnumerable().Count(r => r.Severity == ValidationSeverity.Warning);
            int infos = _results.AsValueEnumerable().Count(r => r.Severity == ValidationSeverity.Info);

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Errors: {errors}  Warnings: {warnings}  Info: {infos}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            if (_results.Count == 0)
            {
                EditorGUILayout.HelpBox("All checks passed.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            IGrouping<string, ValidationResult>[] grouped = _results.AsValueEnumerable().GroupBy(r => r.Category).ToArray();
            foreach (IGrouping<string, ValidationResult> group in grouped)
            {
                string cat = group.Key;
                if (!_categoryFoldouts.ContainsKey(cat))
                    _categoryFoldouts[cat] = true;

                int catErrors = group.AsValueEnumerable().Count(r => r.Severity == ValidationSeverity.Error);
                int catWarnings = group.AsValueEnumerable().Count(r => r.Severity == ValidationSeverity.Warning);
                string header = $"{cat} ({catErrors} errors, {catWarnings} warnings)";

                _categoryFoldouts[cat] = EditorGUILayout.Foldout(_categoryFoldouts[cat], header, true, EditorStyles.foldoutHeader);
                if (!_categoryFoldouts[cat]) continue;

                EditorGUI.indentLevel++;
                foreach (ValidationResult result in group)
                {
                    MessageType msgType = result.Severity switch
                    {
                        ValidationSeverity.Error => MessageType.Error,
                        ValidationSeverity.Warning => MessageType.Warning,
                        _ => MessageType.Info
                    };

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox(result.Message, msgType);
                    if (result.Fix != null)
                    {
                        if (GUILayout.Button("Fix", GUILayout.Width(40), GUILayout.Height(38)))
                        {
                            result.Fix();
                            RunValidation();
                            GUIUtility.ExitGUI();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#nullable disable
