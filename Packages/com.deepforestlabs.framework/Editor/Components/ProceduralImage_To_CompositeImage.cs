using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using ZLinq;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Components
{
	// ReSharper disable once InconsistentNaming
	public static class ProceduralImage_To_CompositeImage
	{
        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Step 1 - Replace (Dry Run)")]
        public static void ReplaceProceduralImagesDryRun()
        {
            ReplaceProceduralImages(true);
        }

        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Step 1 - Replace")]
        public static void ReplaceProceduralImagesApply()
        {
            ReplaceProceduralImages();
        }

        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Step 2 - Bake")]
        public static void BakeAllCompositeImages()
        {
            UICompositeImageAtlasManager.Instance.BakeAllCompositeImages();
        }
        
        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Step 3 - Link")]
        public static void LinkAllCompositeImages()
        {
	        UICompositeImageAtlasManager.Instance.LinkAllCompositeImages();
        }

        
        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Step 4 - Compare")]
        public static void CompareProceduralImagesApply()
        {
	        CompareProceduralImage();
        }

        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Step 5 - Deprecate (Dry Run)")]
        public static void DeprecateFilterBaseDryRun()
        {
            DeprecateFilterBase(true);
        }

        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Step 5 - Deprecate")]
        public static void DeprecateFilterBaseRun()
        {
            DeprecateFilterBase();
        }

        private static void ReplaceProceduralImages(bool dryRun = false)
        {
            string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            //string[] prefabPaths = new []{AssetDatabase.GUIDToAssetPath("1f8c194fd18a05141b497fafaa2b761d")};

            AssetDatabase.StartAssetEditing();

            int total = prefabPaths.Length;
            int current = 0;
            List<(string path, string reason)> skippedPrefabs = new();

            StringBuilder buildReport = new StringBuilder();
            string reportPath = "Assets/EditorLogs/ProceduralImage_Conversion_Report.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

            try
            {
                foreach (string path in prefabPaths)
                {
                    float progress = (float)current / total;
                    string message = $"[{current + 1} / {total}] {progress:P0} - {path}";
                    Log.Info(message);
                    if (EditorUtility.DisplayCancelableProgressBar("Converting Procedural Images", message, progress))
                    {
                        Log.Warn("Conversion cancelled by user.");
                        break;
                    }

                    // Insert root GameObject count check before loading prefab contents
                    GameObject[] roots = AssetDatabase.LoadAllAssetsAtPath(path)
                        .AsValueEnumerable()
                        .OfType<GameObject>()
                        .Where(go => go.transform.parent == null)
                        .ToArray();

                    if (roots.Length != 1)
                    {
                        Debug.LogError($"Skipping {path}: prefab has {roots.Length} root GameObjects.");
                        continue;
                    }

                    GameObject prefabRoot = null;
                    bool loaded = false;
                    try
                    {
	                    prefabRoot = PrefabUtility.LoadPrefabContents(path);
	                    
	                    if (prefabRoot == null)
	                    {
		                    skippedPrefabs.Add(( path, "Could not load prefab root" ));
		                    continue;
	                    }
	                    loaded = true;

	                    Log.Info($"Converting prefab: {path}");

	                    // Find all components and locate those with type name "ProceduralImage"
	                    Component[] allComponents = prefabRoot.GetComponentsInChildren<Component>(true);
	                    List<Component> procImages = new List<Component>();
	                    foreach (Component c in allComponents)
	                    {
	                        if (c != null && c.GetType().Name == "ProceduralImage")
	                        {
		                        procImages.Add(c);
	                        }
	                    }
	                    foreach (Component proc in procImages)
	                    {
	                        if (PrefabUtility.IsPartOfPrefabInstance(proc) &&
	                            !PrefabUtility.IsAddedComponentOverride(proc))
	                        {
	                            Log.Info($"Skipping nested prefab instance: {proc.gameObject.name} in {path}");
	                            continue;
	                        }

	                        GameObject go = proc.gameObject;
	                        Undo.RegisterFullObjectHierarchyUndo(go, "Convert ProceduralImage");

	                        RectTransform rect = proc.GetComponent<RectTransform>();
	                        
	                        Type procType = proc.GetType();
	                        Color color = (Color)procType.GetProperty("color").GetValue(proc);
	                        float border = (float)procType.GetProperty("BorderWidth").GetValue(proc);
	                        float falloff = (float)procType.GetProperty("FalloffDistance").GetValue(proc);
	                        Component modifier = proc.GetComponent($"ProceduralImageModifier");
	                        UICompositeImageModifierType modifierType = UICompositeImageModifierType.Uniform;
	                        if (modifier != null)
	                        {
	                            string modifierTypeName = modifier.GetType().Name;
	                            if (modifierTypeName == "FreeModifier")
	                            {
		                            modifierType = UICompositeImageModifierType.Free;
	                            }
	                            else if (modifierTypeName == "RoundModifier")
	                            {
		                            modifierType = UICompositeImageModifierType.Round;
	                            }
	                            else if (modifierTypeName == "OnlyOneEdgeModifier")
	                            {
		                            modifierType = UICompositeImageModifierType.OnlyOneEdge;
	                            }
	                        }
	                        
	                        // Calculate radius via reflection
	                        Vector4 radii = Vector4.zero;
	                        if (modifier != null && rect != null)
	                        {
	                            MethodInfo method = modifier.GetType().GetMethod("CalculateRadius");
	                            if (method != null)
	                            {
		                            radii = (Vector4)method.Invoke(modifier, new object[] { rect.rect });
	                            }
	                        }

	                        Component[] comps = proc.gameObject.GetComponents<Component>();
	                        foreach (Component comp in comps)
	                        {
		                        if (comp == null)
		                        {
			                        break;
		                        }

		                        switch (comp.GetType().Name)
		                        {
			                        case "FreeModifier":
			                        case "OnlyOneEdgeModifier":
			                        case "RoundModifier":
			                        case "UniformModifier":
				                        Object.DestroyImmediate(comp);
				                        break;
		                        }
	                        }
	                        Object.DestroyImmediate(proc, true);
	                        UICompositeImage newImage = go.AddComponent<UICompositeImage>();
	                        
	                        SerializedObject so = new SerializedObject(newImage);
	                        so.FindProperty("_mode").enumValueIndex = (int)UICompositeImageRenderMode.Dynamic;
	                        so.FindProperty("_borderWidth").floatValue = border;
	                        so.FindProperty("_falloffDistance").floatValue = falloff;
	                        so.FindProperty("_modifier").enumValueIndex = (int)modifierType;
	                        so.FindProperty("_radii").vector4Value = radii;
	                        so.FindProperty("m_Color").colorValue = color;

	                        SerializedProperty layers = so.FindProperty("_layers");
	                        layers.arraySize = 1;
	                        SerializedProperty layerProp = layers.GetArrayElementAtIndex(0);
	                        layerProp.FindPropertyRelative("_enabled").boolValue = true;
	                        layerProp.FindPropertyRelative("_sprite").objectReferenceValue = null;
	                        layerProp.FindPropertyRelative("_color").colorValue = color;
	                        layerProp.FindPropertyRelative("_fillCenter").boolValue = true;

	                        // Copy over mesh effects.
	                        BaseMeshEffect[] effects = go.GetComponents<BaseMeshEffect>();
	                        if (effects != null && effects.Length > 0)
	                        {
	                            for (int l = 0; l < layers.arraySize; l++)
	                            {
	                                SerializedProperty effectsProp = layerProp.FindPropertyRelative("_effects");
	                                effectsProp.ClearArray();
	                                effectsProp.arraySize = effects.Length;
	                                for (int i = 0; i < effects.Length; i++)
	                                {
	                                    effectsProp.GetArrayElementAtIndex(i).objectReferenceValue = effects[i];
	                                }
	                            }
	                        }

	                        so.ApplyModifiedPropertiesWithoutUndo();
	                    }
                    }
                    catch (Exception ex)
                    {
	                    Debug.LogException(ex);
	                    string err = $"Error converting prefab at {path}:\n{ex}";
	                    skippedPrefabs.Add(( path, err ));
                    }
                    finally
                    {
	                    if (loaded)
	                    {
		                    if (dryRun)
		                    {
			                    Log.Info($"[DryRun] Would save prefab: {path}");
		                    }
		                    else
		                    {
			                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
		                    }
		                    
		                    PrefabUtility.UnloadPrefabContents(prefabRoot);
	                    }
                    }

                    current++;
                }
                // After prefab and scene processing loops, but before EditorUtility.ClearProgressBar();
                if (skippedPrefabs.Count > 0)
                {
                    string skippedPrefabLog = $"Skipped {skippedPrefabs.Count} prefab(s):\n" + string.Join("\n", skippedPrefabs);
                    Log.Warn(skippedPrefabLog);
                    buildReport.AppendLine(skippedPrefabLog).AppendLine();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            
            AssetDatabase.StopAssetEditing();
            
            Log.Info("ProceduralImage conversion complete.");
            buildReport.AppendLine("ProceduralImage conversion complete.");
            File.WriteAllText(reportPath, buildReport.ToString());
            Log.Info($"Build report saved to: {reportPath}");
            EditorUtility.RevealInFinder(reportPath);
        }
        
        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Validate")]
        public static void ValidateConversionComplete()
        {
            string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            string[] scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
            List<string> references = new List<string>();

            int total = prefabPaths.Length + scenePaths.Length;
            int current = 0;

            foreach (string path in prefabPaths)
            {
                float progress = (float)current / total;
                string message = $"[{current + 1}/{total}] {progress:P0} - {path}";
                if (EditorUtility.DisplayCancelableProgressBar("Validating Conversion", message, progress))
                {
                    Log.Warn("Validation cancelled by user.");
                    EditorUtility.ClearProgressBar();
                    return;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
	                // Use reflection to check for ProceduralImage or FilterBase
	                bool found = false;
	                foreach (Component c in prefab.GetComponentsInChildren<Component>(true))
	                {
	                    if (c != null)
	                    {
	                        string n = c.GetType().Name;
	                        if (n == "ProceduralImage" || n == "FilterBase")
	                        {
	                            found = true;
	                            break;
	                        }
	                    }
	                }

	                if (found)
	                {
		                references.Add(path);
	                }
                }
                current++;
            }

            foreach (string path in scenePaths)
            {
                float progress = (float)current / total;
                string message = $"[{current + 1}/{total}] {progress:P0} - {path}";
                if (EditorUtility.DisplayCancelableProgressBar("Validating Conversion", message, progress))
                {
                    Log.Warn("Validation cancelled by user.");
                    EditorUtility.ClearProgressBar();
                    return;
                }

                try
                {
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                // Use reflection to check for ProceduralImage or FilterBase in scene
                bool found = false;
                foreach (Component c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include))
                {
                    if (c != null)
                    {
                        string n = c.GetType().Name;
                        if (n == "ProceduralImage" || n == "FilterBase")
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found) { references.Add(path); }
                    EditorSceneManager.CloseScene(scene, true);
                }
                catch
                {
                    references.Add($"Failed to validate scene: {path}");
                }
                current++;
            }

            EditorUtility.ClearProgressBar();

            if (references.Count > 0)
            {
                string message = "Conversion incomplete. Found references to ProceduralImage or FilterBase in:\n" +
                    string.Join("\n", references);
                Log.Warn(message);
                EditorUtility.DisplayDialog("Validation Failed", message, "OK");
            }
            else
            {
                string message = "Validation successful: No remaining ProceduralImage or FilterBase components found.";
                Log.Info(message);
                EditorUtility.DisplayDialog("Validation Successful", message, "OK");
            }
        }

        /// <summary>
        /// Removes all FilterBase components from GameObjects containing UICompositeImage and matching UIDefaultImageLayer,
        /// logs each removal, and writes a summary to the build report.
        /// </summary>
        /// <param name="dryRun"></param>
        public static void DeprecateFilterBase(bool dryRun = false)
        {
	        throw new NotImplementedException("Needs work.");
            /*string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            string[] scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);

            AssetDatabase.StartAssetEditing();

            int total = prefabPaths.Length;
            int current = 0;
            List<string> skippedPrefabs = new List<string>();
            Dictionary<string, int> filterTypeCounts = new Dictionary<string, int>();
            StringBuilder removalLog = new StringBuilder();
            string reportPath = "Assets/EditorLogs/ProceduralImage_DeprecateFilters_Report.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

            try
            {
                foreach (string path in prefabPaths)
                {
                    float progress = (float)current / total;
                    string message = $"[Deprecate] [{current + 1} / {total}] {progress:P0} - {path}";
                    if (EditorUtility.DisplayCancelableProgressBar("Deprecating FilterBase components", message, progress))
                    {
                        Log.Warn("Deprecation cancelled by user.");
                        break;
                    }

                    GameObject prefabRoot = null;
                    bool loaded = false;
                    try
                    {
	                    prefabRoot = PrefabUtility.LoadPrefabContents(path);
	                    if (prefabRoot == null)
	                    {
		                    skippedPrefabs.Add(path);
		                    continue;
	                    }

	                    loaded = true;
                    }
                    catch (Exception ex)
                    {
	                    Debug.LogError($"Error deprecating filters in prefab at {path}:\n{ex}");
	                    skippedPrefabs.Add(path);
                    }
                    finally
                    {
	                    if (loaded)
	                    {
		                    if (dryRun)
		                    {
			                    Log.Info($"[DryRun] Would save prefab (filters removed): {path}");
		                    }
		                    else
		                    {
			                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
		                    }
	                    }
                    }
                    current++;
                }

                if (skippedPrefabs.Count > 0)
                {
                    string skippedPrefabLog = $"Skipped {skippedPrefabs.Count} prefab(s):\n" + string.Join("\n", skippedPrefabs);
                    removalLog.AppendLine(skippedPrefabLog).AppendLine();
                }

                // Now process scenes
                List<string> sceneErrors = new List<string>();
                for (int i = 0; i < scenePaths.Length; i++)
                {
                    string scenePath = scenePaths[i];
                    float progress = (float)i / scenePaths.Length;
                    string message = $"[Deprecate Scene {i + 1} / {scenePaths.Length}] {progress:P0} - {scenePath}";
                    if (EditorUtility.DisplayCancelableProgressBar("Deprecating FilterBase components (Scenes)", message, progress))
                    {
                        Log.Warn("Scene deprecation cancelled by user.");
                        break;
                    }
                    try
                    {
                        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                        bool sceneModified = false;

                        if (sceneModified && !dryRun)
                        {
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                        }
                        EditorSceneManager.CloseScene(scene, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error deprecating filters in scene at {scenePath}:\n{ex}");
                        sceneErrors.Add(scenePath);
                    }
                }

                // Write summary to build report
                removalLog.AppendLine();
                removalLog.AppendLine("FilterBase Removal Summary:");
                foreach (KeyValuePair<string, int> kvp in filterTypeCounts)
                {
                    removalLog.AppendLine($"Removed {kvp.Value} instances of {kvp.Key}");
                }
                if (filterTypeCounts.Count == 0)
                {
	                removalLog.AppendLine("No FilterBase components were removed.");
                }

                // Append scene errors if any
                if (sceneErrors.Count > 0)
                {
                    removalLog.AppendLine();
                    removalLog.AppendLine("Scenes with errors during deprecation:");
                    foreach (string scenePath in sceneErrors)
                    {
                        removalLog.AppendLine($"- {scenePath}");
                    }
                }

                File.WriteAllText(reportPath, removalLog.ToString());
                Log.Info($"DeprecateFilterBase complete. Removal summary written to: {reportPath}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }*/
        }

        private static void CompareProceduralImage()
        {
            const float matchThreshold = 0.995f;
            List<string> failedMatches = new List<string>();

            string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            int total = prefabPaths.Length;
            int current = 0;

            StringBuilder results = new StringBuilder();
            results.AppendLine("ProceduralImage Comparison Report:");

            int width = 1920;
            int height = 1024;

            foreach (string path in prefabPaths)
            {
                float progress = (float)current / total;
                string message = $"[{current + 1} / {total}] {progress:P0} - {path}";

                if (EditorUtility.DisplayCancelableProgressBar("Comparing Converted Prefabs", message, progress))
                {
                    Debug.LogWarning("Comparison cancelled.");
                    break;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    results.AppendLine($"- {path}: Could not load prefab.");
                    current++;
                    continue;
                }

                UICompositeImage composite = prefab.GetComponentInChildren<UICompositeImage>();
                if (composite == null)
                {
                    results.AppendLine($"- {path}: No UICompositeImage found.");
                    current++;
                    continue;
                }

                RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                rt.filterMode = FilterMode.Point;

                Camera cam = new GameObject("TempCam").AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = height / 2f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.clear;
                cam.targetTexture = rt;

                Canvas canvas = new GameObject("TempCanvas").AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.pixelPerfect = false;

                GameObject instance = GameObject.Instantiate(prefab, canvas.transform);

                cam.Render();

                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                int matchCount = 0;
                int pixelCount = width * height;
                Color32[] pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].a > 0)
                    {
                        matchCount++;
                    }
                }

                float score = (float)matchCount / pixelCount * 100f;
                results.AppendLine($"- {path}: Alpha Fill Score = {score:0.0}%");
                if (score < matchThreshold * 100f)
                {
                    failedMatches.Add($"FAILED [{score:0.0}%] {path} (object: {composite.gameObject.name})");
                }

                Object.DestroyImmediate(instance);
                Object.DestroyImmediate(canvas.gameObject);
                Object.DestroyImmediate(cam.gameObject);
                RenderTexture.active = null;
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);

                current++;
            }

            EditorUtility.ClearProgressBar();

            if (failedMatches.Count > 0)
            {
                results.AppendLine();
                results.AppendLine("Failed Matches:");
                foreach (string failure in failedMatches)
                {
                    results.AppendLine(failure);
                }
            }

            string reportPath = "Assets/EditorLogs/ProceduralImage_Compare_Report.txt";
            File.WriteAllText(reportPath, results.ToString());
            Debug.Log($"Comparison complete. Report saved to: {reportPath}");
            EditorUtility.RevealInFinder(reportPath);
        }
        
        [MenuItem("Tools/Composite Image/Conversion/ProceduralImage/Patches/Sprite Type Patch")]
        public static void SpriteTypePatch()
        {
            AssetDatabase.StartAssetEditing();
            EditorApplication.update += () => {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            };
            
            string exportDirectoryPath = AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.SearchForAssetsDirectory);
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { exportDirectoryPath });
            //string[] prefabGuids = new[] {"822f8011803914bd48906a48a450128a"};
            int total = prefabGuids.Length;
            int current = 0;

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(prefabPath))
                {
                    continue;
                }
                float progress = (float)current / total;
                if (EditorUtility.DisplayCancelableProgressBar("Baking Composite Images", prefabPath, progress * 0.9f))
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                    return;
                }

                GameObject prefabRoot = null;
                try
                {
                    prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                    if (prefabRoot == null)
                    {
                        current++;
                        continue;
                    }
                    SpriteTypePatchRecursive(prefabRoot.transform);
                }
                finally
                {
                    if (prefabRoot != null)
                    {
	                    // 3) Save your changes back to the asset
	                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                    current++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
        }
        
        private static void SpriteTypePatchRecursive(Transform current)
        {
	        // 1) Process any components on this node:
	        UICompositeImage[] components = current.GetComponents<UICompositeImage>();
	        if (components != null && components.Length > 0)
	        {
		        foreach (UICompositeImage component in components)
		        {
			        SerializedObject so = new SerializedObject(component);
			        SerializedProperty typeProp = so.FindProperty("m_Type");
			        SerializedProperty modifierProp = so.FindProperty("_modifier");
			        switch ((UICompositeImageModifierType)modifierProp.enumValueIndex)
			        {
				        case UICompositeImageModifierType.Round:
					        typeProp.enumValueIndex = (int)Image.Type.Simple;
					        break;
				        default:
					        typeProp.enumValueIndex = (int)Image.Type.Sliced;
					        break;
			        }
			        so.ApplyModifiedPropertiesWithoutUndo();
		        }
	        }

	        // 2) Recurse into all children:
	        foreach (Transform child in current)
	        {
		        SpriteTypePatchRecursive(child);
	        }
        }
        
        private static class Log
        {
	        [System.Diagnostics.Conditional("NOT_RELEASE_BUILD")]
	        public static void Info(string message)
	        {
		        Debug.Log(message);
	        }

	        [System.Diagnostics.Conditional("NOT_RELEASE_BUILD")]
	        public static void Warn(string message)
	        {
		        Debug.LogWarning(message);
	        }
        }
	}
}