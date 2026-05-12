#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using DeepForestLabs.Factories;
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.EditorTools.Validation
{
    public static class ProjectValidator
    {
        public static List<ValidationResult> ValidateProject()
        {
            List<ValidationResult> results = new();
            ValidateCoreAssets(results);
            ValidateBuildSystem(results);
            ValidateAudio(results);
            return results;
        }

        private static void ValidateCoreAssets(List<ValidationResult> results)
        {
            MainArgs? mainArgs = Resources.Load<MainArgs>(Main.MAIN_ARGS_RESOURCES_PATH);
            if (mainArgs == null)
            {
                results.Add(new ValidationResult(
                    ValidationSeverity.Error,
                    "MainArgs asset not found at Resources/MainArgs.",
                    "Core",
                    () => CreateAssetIfTypeExists<MainArgs>("Assets/Resources/MainArgs.asset")));
                return;
            }

            SerializedObject so = new SerializedObject(mainArgs);
            CheckFieldAssigned(so, "_logFilter", "MainArgs.LogFilter", "Core", results);
            CheckFieldAssigned(so, "_appContainerFactory", "MainArgs.AppContainerFactory", "Core", results);

            ContainerBuilderFactory? appFactory = FindAppFactory();
            if (appFactory == null)
            {
                results.Add(new ValidationResult(
                    ValidationSeverity.Warning,
                    "No AppContainerFactory asset found in Resources.",
                    "Core"));
            }
            else
            {
                SerializedObject factorySo = new SerializedObject(appFactory);
                SerializedProperty iter = factorySo.GetIterator();
                if (iter.NextVisible(true))
                {
                    do
                    {
                        if (iter.propertyType == SerializedPropertyType.ObjectReference &&
                            iter.objectReferenceValue == null &&
                            iter.name != "m_Script")
                        {
                            results.Add(new ValidationResult(
                                ValidationSeverity.Warning,
                                $"AppContainerFactory field '{iter.displayName}' is unassigned.",
                                "Core"));
                        }
                    } while (iter.NextVisible(false));
                }
            }
        }

        private static void ValidateBuildSystem(List<ValidationResult> results)
        {
            ScriptableObject? buildSystemSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/Editor/BuildSystemSettings.asset");

            if (buildSystemSettings == null)
            {
                results.Add(new ValidationResult(
                    ValidationSeverity.Error,
                    "BuildSystemSettings not found at Assets/Editor/BuildSystemSettings.asset.",
                    "Build System"));
            }
            else
            {
                SerializedObject so = new SerializedObject(buildSystemSettings);
                SerializedProperty appName = so.FindProperty("_appName");
                if (appName != null && string.IsNullOrWhiteSpace(appName.stringValue))
                {
                    results.Add(new ValidationResult(
                        ValidationSeverity.Warning,
                        "BuildSystemSettings: App Name is empty.",
                        "Build System"));
                }

                SerializedProperty envUrl = so.FindProperty("_environmentsUrl");
                if (envUrl != null && (string.IsNullOrEmpty(envUrl.stringValue) ||
                    envUrl.stringValue.Contains("example.com")))
                {
                    results.Add(new ValidationResult(
                        ValidationSeverity.Warning,
                        "BuildSystemSettings: Environments URL is the placeholder default.",
                        "Build System"));
                }
            }

            ScriptableObject? buildSettings = Resources.Load<ScriptableObject>("BuildSettings");
            if (buildSettings == null)
            {
                results.Add(new ValidationResult(
                    ValidationSeverity.Warning,
                    "BuildSettings not found at Resources/BuildSettings. It should be auto-created on editor load.",
                    "Build System"));
            }
        }

        private static void ValidateAudio(List<ValidationResult> results)
        {
            string[] mixerConfigs = AssetDatabase.FindAssets("t:AudioMixerConfig");
            if (mixerConfigs.Length == 0)
            {
                results.Add(new ValidationResult(
                    ValidationSeverity.Info,
                    "No AudioMixerConfig asset found. Create one if using the audio package.",
                    "Audio"));
                return;
            }

            string configPath = AssetDatabase.GUIDToAssetPath(mixerConfigs[0]);
            ScriptableObject? config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(configPath);
            if (config != null)
            {
                SerializedObject so = new SerializedObject(config);
                SerializedProperty mixer = so.FindProperty("_mixer");
                if (mixer != null && mixer.objectReferenceValue == null)
                {
                    results.Add(new ValidationResult(
                        ValidationSeverity.Error,
                        "AudioMixerConfig has no AudioMixer assigned.",
                        "Audio"));
                }
            }

            string[] catalogs = AssetDatabase.FindAssets("t:SoundCatalog");
            foreach (string guid in catalogs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject? catalog = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (catalog == null) continue;

                SerializedObject so = new SerializedObject(catalog);
                SerializedProperty entries = so.FindProperty("_entries");
                if (entries == null) continue;

                HashSet<string> keys = new();
                for (int i = 0; i < entries.arraySize; i++)
                {
                    SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                    string key = entry.FindPropertyRelative("_key")?.stringValue ?? string.Empty;
                    if (string.IsNullOrEmpty(key))
                    {
                        results.Add(new ValidationResult(
                            ValidationSeverity.Warning,
                            $"SoundCatalog '{path}': entry {i} has empty key.",
                            "Audio"));
                    }
                    else if (!keys.Add(key))
                    {
                        results.Add(new ValidationResult(
                            ValidationSeverity.Error,
                            $"SoundCatalog '{path}': duplicate key '{key}'.",
                            "Audio"));
                    }
                }
            }
        }

        private static void CheckFieldAssigned(SerializedObject so, string fieldName, string displayName,
            string category, List<ValidationResult> results)
        {
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null) return;

            if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == null)
            {
                results.Add(new ValidationResult(
                    ValidationSeverity.Warning,
                    $"{displayName} is not assigned.",
                    category));
            }
            else
            {
                SerializedProperty guidProp = prop.FindPropertyRelative("_guid");
                SerializedProperty resProp = prop.FindPropertyRelative("_resourcesPath");
                if (guidProp != null && resProp != null &&
                    string.IsNullOrEmpty(guidProp.stringValue) && string.IsNullOrEmpty(resProp.stringValue))
                {
                    results.Add(new ValidationResult(
                        ValidationSeverity.Warning,
                        $"{displayName} asset ref is empty.",
                        category));
                }
            }
        }

        private static ContainerBuilderFactory? FindAppFactory()
        {
            string[] guids = AssetDatabase.FindAssets("t:ContainerBuilderFactory", new[] { "Assets/Resources" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ContainerBuilderFactory? factory = AssetDatabase.LoadAssetAtPath<ContainerBuilderFactory>(path);
                if (factory != null && factory is not MainArgs)
                    return factory;
            }
            return null;
        }

        private static void CreateAssetIfTypeExists<T>(string path) where T : ScriptableObject
        {
            Type? concreteType = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (!t.IsAbstract && typeof(T).IsAssignableFrom(t))
                    {
                        concreteType = t;
                        break;
                    }
                }
                if (concreteType != null) break;
            }

            if (concreteType == null)
            {
                Debug.LogWarning($"No concrete subclass of {typeof(T).Name} found to create.");
                return;
            }

            ScriptableObject instance = ScriptableObject.CreateInstance(concreteType);
            string dir = System.IO.Path.GetDirectoryName(path)!;
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = instance;
        }
    }
}
#nullable disable
