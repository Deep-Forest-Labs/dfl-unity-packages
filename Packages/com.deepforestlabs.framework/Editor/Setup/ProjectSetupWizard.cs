#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DeepForestLabs.EditorTools.Validation;
using DeepForestLabs.Factories;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.EditorTools.Setup
{
    public sealed class ProjectSetupWizard : EditorWindow
    {
        private int _currentStep;
        private Vector2 _scrollPos;
        private List<ValidationResult>? _validationResults;

        private static readonly string[] StepNames = { "Core Assets", "Build System", "Audio", "Validation" };

        [MenuItem("Deep Forest Labs/Tools/Project Setup Wizard")]
        public static void ShowWindow()
        {
            ProjectSetupWizard window = GetWindow<ProjectSetupWizard>("DFL Project Setup");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            DrawStepHeader();
            EditorGUILayout.Space();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_currentStep)
            {
                case 0: DrawCoreAssetsStep(); break;
                case 1: DrawBuildSystemStep(); break;
                case 2: DrawAudioStep(); break;
                case 3: DrawValidationStep(); break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawNavigation();
        }

        private void DrawStepHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            for (int i = 0; i < StepNames.Length; i++)
            {
                bool isCurrent = i == _currentStep;
                GUIStyle style = isCurrent ? EditorStyles.toolbarButton : EditorStyles.toolbarButton;
                GUI.enabled = true;
                if (GUILayout.Toggle(isCurrent, $" {i + 1}. {StepNames[i]} ", style))
                {
                    _currentStep = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNavigation()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _currentStep > 0;
            if (GUILayout.Button("Previous", GUILayout.Height(30)))
                _currentStep--;
            GUI.enabled = _currentStep < StepNames.Length - 1;
            if (GUILayout.Button("Next", GUILayout.Height(30)))
                _currentStep++;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCoreAssetsStep()
        {
            EditorGUILayout.LabelField("Core Assets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These assets are required for the DFL framework to run.",
                MessageType.Info);
            EditorGUILayout.Space();

            MainArgs? mainArgs = Resources.Load<MainArgs>(Main.MAIN_ARGS_RESOURCES_PATH);
            DrawAssetStatus("MainArgs", "Assets/Resources/MainArgs.asset", mainArgs != null,
                () => CreateAssetByTypeName("MainArgs", "Assets/Resources/MainArgs.asset"));

            ContainerBuilderFactory? appFactory = FindAppFactory();
            DrawAssetStatus("AppContainerFactory", "Assets/Resources/AppContainer.asset", appFactory != null,
                () => CreateConcreteAsset<ContainerBuilderFactory>("Assets/Resources/AppContainer.asset"));

            ScriptableObject? logFilter = Resources.Load<ScriptableObject>("LogFilter");
            DrawAssetStatus("LogFilter", "Assets/Resources/LogFilter.asset", logFilter != null,
                () => CreateAssetByTypeName("LogFilter", "Assets/Resources/LogFilter.asset"));
        }

        private void DrawBuildSystemStep()
        {
            EditorGUILayout.LabelField("Build System", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Build system assets configure CI/CD and project identity.",
                MessageType.Info);
            EditorGUILayout.Space();

            ScriptableObject? buildSystemSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/Editor/BuildSystemSettings.asset");
            DrawAssetStatus("BuildSystemSettings", "Assets/Editor/BuildSystemSettings.asset",
                buildSystemSettings != null,
                () => CreateAssetByTypeName("BuildSystemSettings", "Assets/Editor/BuildSystemSettings.asset"));

            ScriptableObject? buildSettings = Resources.Load<ScriptableObject>("BuildSettings");
            DrawAssetStatus("BuildSettings", "Assets/Resources/BuildSettings.asset", buildSettings != null, null);

            ScriptableObject? filesToCleanup = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/Scripts/BuildSystem/Editor/FilesToCleanup.asset");
            DrawAssetStatus("FilesToCleanup", "Assets/Scripts/BuildSystem/Editor/FilesToCleanup.asset",
                filesToCleanup != null,
                () => CreateAssetByTypeName("FilesToCleanup",
                    "Assets/Scripts/BuildSystem/Editor/FilesToCleanup.asset"));
        }

        private void DrawAudioStep()
        {
            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Audio assets configure the mixer, sound catalog, and group registry. " +
                "You must first create a Unity AudioMixer with Master/BGM/SFX/UI groups.",
                MessageType.Info);
            EditorGUILayout.Space();

            string[] mixerConfigs = AssetDatabase.FindAssets("t:AudioMixerConfig");
            DrawAssetStatus("AudioMixerConfig", "(any location)", mixerConfigs.Length > 0,
                () => CreateAssetByTypeName("AudioMixerConfig", "Assets/Resources/AudioMixerConfig.asset"));

            string[] catalogs = AssetDatabase.FindAssets("t:SoundCatalog");
            DrawAssetStatus("SoundCatalog", "(any location)", catalogs.Length > 0,
                () => CreateAssetByTypeName("SoundCatalog", "Assets/Resources/SoundCatalog.asset"));

            string[] registries = AssetDatabase.FindAssets("t:SoundGroupRegistry");
            DrawAssetStatus("SoundGroupRegistry", "(any location)", registries.Length > 0,
                () => CreateAssetByTypeName("SoundGroupRegistry", "Assets/Resources/SoundGroupRegistry.asset"));
        }

        private void DrawValidationStep()
        {
            EditorGUILayout.LabelField("Project Validation", EditorStyles.boldLabel);

            if (GUILayout.Button("Run Validation", GUILayout.Height(30)))
            {
                _validationResults = ProjectValidator.ValidateProject();
            }

            if (_validationResults == null)
            {
                EditorGUILayout.HelpBox("Click 'Run Validation' to check your project setup.", MessageType.Info);
                return;
            }

            if (_validationResults.Count == 0)
            {
                EditorGUILayout.HelpBox("All checks passed!", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            foreach (ValidationResult result in _validationResults)
            {
                MessageType msgType = result.Severity switch
                {
                    ValidationSeverity.Error => MessageType.Error,
                    ValidationSeverity.Warning => MessageType.Warning,
                    _ => MessageType.Info
                };

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox($"[{result.Category}] {result.Message}", msgType);
                if (result.Fix != null && GUILayout.Button("Fix", GUILayout.Width(40), GUILayout.Height(38)))
                {
                    result.Fix();
                    _validationResults = ProjectValidator.ValidateProject();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawAssetStatus(string label, string path, bool exists, Action? createAction)
        {
            EditorGUILayout.BeginHorizontal();

            GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
            if (exists)
            {
                EditorGUILayout.LabelField($"  {label}", statusStyle);
                EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
            }
            else
            {
                statusStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
                EditorGUILayout.LabelField($"  {label}", statusStyle);
                EditorGUILayout.LabelField("MISSING", EditorStyles.miniLabel);

                if (createAction != null && GUILayout.Button("Create", GUILayout.Width(60)))
                {
                    createAction();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static ContainerBuilderFactory? FindAppFactory()
        {
            string[] guids = AssetDatabase.FindAssets("t:ContainerBuilderFactory",
                new[] { "Assets/Resources" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ContainerBuilderFactory? factory =
                    AssetDatabase.LoadAssetAtPath<ContainerBuilderFactory>(path);
                if (factory != null && factory is not MainArgs)
                    return factory;
            }
            return null;
        }

        private static void CreateConcreteAsset<T>(string path) where T : ScriptableObject
        {
            Type? typeToCreate = FindConcreteType(typeof(T), t => !typeof(MainArgs).IsAssignableFrom(t));
            if (typeToCreate == null)
            {
                Debug.LogWarning($"No concrete subclass of {typeof(T).Name} found. Create one first.");
                return;
            }

            EnsureDirectory(path);
            ScriptableObject instance = ScriptableObject.CreateInstance(typeToCreate);
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = instance;
        }

        private static void CreateAssetByTypeName(string typeName, string path)
        {
            Type? type = FindTypeByName(typeName);
            if (type == null)
            {
                Debug.LogWarning($"Type '{typeName}' not found or is abstract. " +
                                 "Ensure the package containing it is installed.");
                return;
            }

            EnsureDirectory(path);
            ScriptableObject instance = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = instance;
        }

        private static Type? FindConcreteType(Type baseType, Func<Type, bool>? extraFilter = null)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (!t.IsAbstract && baseType.IsAssignableFrom(t) &&
                        (extraFilter == null || extraFilter(t)))
                        return t;
                }
            }
            return null;
        }

        private static Type? FindTypeByName(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.Name == typeName && !t.IsAbstract && typeof(ScriptableObject).IsAssignableFrom(t))
                        return t;
                }
            }
            return null;
        }

        private static void EnsureDirectory(string assetPath)
        {
            string? dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
#nullable disable
