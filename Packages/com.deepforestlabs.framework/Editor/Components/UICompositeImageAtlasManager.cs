#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using DeepForestLabs.Logger;
using ZLinq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Components
{
    internal sealed class UICompositeImageAtlasManager
    {
        private static readonly HashSet<string> _overwrittenThisRun = new();
        private static UICompositeImageAtlasManager? _instance;
        public static UICompositeImageAtlasManager Instance => _instance ??= new UICompositeImageAtlasManager();
        
        private readonly List<(UICompositeImage, string, UICompositeImageLayerType)> _pendingLinks = new();
        
        public void BakeAllCompositeImages()
        {
            AssetDatabase.StartAssetEditing();
            EditorApplication.update += () => {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            };

            _pendingLinks.Clear();
            string exportDirectoryPath = AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.SearchForAssetsDirectory);
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { exportDirectoryPath });
            //string[] prefabGuids = new[] {"714cea93186de4f1f95b5e6ecbd87051"};
            int total = prefabGuids.Length;
            int current = 0;
            List<string> builtSprites = new();

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

                GameObject? prefabRoot = null;
                try
                {
                    prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                    if (prefabRoot == null)
                    {
                        current++;
                        continue;
                    }
                    if (prefabRoot.transform is not RectTransform) // TODO - this might miss a few, but should speed up the tool significantly by skipping 3D content
                    {
                        current++;
                        continue;
                    }
                    Log.Error($"try - current: {prefabPath} (total: {current})");
                    BakeRecursive(prefabRoot.transform, builtSprites, prefabPath);
                }
                finally
                {
                    if (prefabRoot != null)
                    {
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                    Log.Error($"finally - current: {prefabPath} (isNull: {prefabRoot == null}, total: {current})");
                    current++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
        }
        
        private void BakeRecursive(Transform current, List<string> builtSprites, string prefabPath)
        {
            // 1) Process any components on this node:
            UICompositeImage[] components = current.GetComponents<UICompositeImage>();
            if (components != null && components.Length > 0)
            {
                foreach (UICompositeImage component in components)
                {
                    SerializedObject so = new SerializedObject(component);
                    PrepareCompositeForEffectOnlyExport(component);
                    Build(component, builtSprites, prefabPath);
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // 2) Recurse into all children:
            foreach (Transform child in current)
            {
                BakeRecursive(child, builtSprites, prefabPath);
            }
        }
        
        public void LinkAllCompositeImages()
        {
            string exportDirectoryPath = AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.SearchForAssetsDirectory);
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { exportDirectoryPath });
            //string[] prefabGuids = new[] { "c7e77ee4caa107a4d8a8ffcd919bb542" };
            //string[] prefabGuids = new[] {"f72a934f71ad0e54dbd488af9c52d9f6"};
            int total = prefabGuids.Length;

            try
            {
                for (int i = 0; i < total; i++)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                    float progress = (float)i / (total - 1);
                    if (EditorUtility.DisplayCancelableProgressBar("Linking Composite Images", prefabPath, progress))
                    {
                        break;
                    }

                    GameObject? prefab = null;
                    try
                    {
                        // 1) Load into isolated edit‐scope
                        prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                        // 2) Mutate all your UICompositeImage components
                        LinkRecursive(prefab.transform, prefab.transform);

                        // 3) Save your changes back to the asset
                        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Linking failed on '{prefabPath}': {ex.Message}");
                    }
                    finally
                    {
                        // 4) Always unload, even if SaveAsPrefabAsset threw:
                        if (prefab != null)
                        {
                            PrefabUtility.UnloadPrefabContents(prefab);
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets(); // may be unncessary
            }
        }
        
        private static void LinkRecursive(Transform initial, Transform current, UICompositeImage[]? components = null)
        {
            Log.Editor($"{current.name} - {components != null}");
            components ??= current.GetComponents<UICompositeImage>();
            if (components != null && components.Length > 0)
            {
                foreach (UICompositeImage component in components)
                {
                    Log.Editor($"{current} - {component.gameObject.name} - linking now");
                    string spriteName = GetSpriteName(component, UICompositeImageLayerType.Default);
                    string fullPath = Path.Combine(AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.ExportDirectory), spriteName);
                    string assetPath = fullPath.Replace(Application.dataPath, "Assets");
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite == null)
                    {
                        Log.EditorError("Missing UICompositeImage sprite '{0}'", fullPath);
                        continue;
                    }

                    SerializedObject so = new SerializedObject(component);
                    SerializedProperty layersProp = so.FindProperty("_layers");
                    if (layersProp.arraySize == 0)
                    {
                        layersProp.InsertArrayElementAtIndex(0);
                    }
                    SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(0);
                    layerProp.FindPropertyRelative("_fillCenter").boolValue = true; // force FillCenter to true, since we aren't using this for now
                    SerializedProperty spriteProp = layerProp.FindPropertyRelative("_sprite");
                    spriteProp.objectReferenceValue = sprite;
                    //modeProp.enumValueIndex = HasEffects(component.gameObject)
                    //    ? (int)UICompositeImageRenderMode.Dynamic
                    //    : (int)UICompositeImageRenderMode.BakedVertex;
                    SerializedProperty modeProp = so.FindProperty("_mode");
                    modeProp.enumValueIndex = (int)UICompositeImageRenderMode.BakedVertex; // just use BakedVertex for now, we don't have any effects being used afaik

                    // 1) Apply and mark dirty so Unity knows to save:
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(component);
                    EditorSceneManager.MarkSceneDirty(component.gameObject.scene);

                    // 2) Force the Image to re‐read its layers:
                    component.RefreshSpriteMaterial();
                    component.SetAllDirty();
                }
            }

            foreach (Transform child in current)
            {
                Log.Editor($"{child.name} child");
                UICompositeImage[]? childComponents = null; // provide an array of child components, when we only want to modify overrides
                bool shouldSkip = false;
                UICompositeImage[] nestedComponents = GetNestedPrefabComponents(child.gameObject);
                if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject))
                {
                    shouldSkip = true;
                    // check for overrides
                    if (PrefabUtility.HasPrefabInstanceAnyOverrides(child.gameObject, false))
                    {
                        foreach (UICompositeImage comp in nestedComponents)
                        {
                            // if we've added any components as overrides, do not skip
                            if (PrefabUtility.IsAddedComponentOverride(comp))
                            {
                                shouldSkip = false;
                                Log.Editor($"{child.name} - {comp.gameObject.name} - added UICompositeImage");
                                if (childComponents == null)
                                {
                                    childComponents = new[] { comp };
                                }
                                else
                                {
                                    childComponents.AsValueEnumerable().Append(comp);
                                }
                            }

                            // if the component is on an added gameobject override, or any of its parents are added overrides, do not skip
                            Transform parent = comp.transform;
                            while (parent != null)
                            {
                                if (PrefabUtility.IsAddedGameObjectOverride(parent.gameObject))
                                {
                                    shouldSkip = false;
                                    Log.Editor($"{child.name} - {comp.gameObject.name} - added {parent.gameObject.name}");
                                    if (childComponents == null)
                                    {
                                        childComponents = new[] { comp };
                                    }
                                    else
                                    {
                                        childComponents.AsValueEnumerable().Append(comp);
                                    }
                                    break;
                                }

                                parent = parent.parent;
                            }
                        }

                        // if we've overridden any properties on the components in question, do not skip
                        List<ObjectOverride> overrides = PrefabUtility.GetObjectOverrides(child.gameObject, false);
                        foreach (ObjectOverride o in overrides)
                        {
                            UICompositeImage? comp = o.instanceObject as UICompositeImage;
                            if (comp != null)
                            {
                                shouldSkip = false;
                                Log.Editor($"{child.name} - {comp.gameObject.name} - overridden property");
                                if (childComponents == null)
                                {
                                    childComponents = new[] { comp };
                                }
                                else
                                {
                                    childComponents.AsValueEnumerable().Append(comp);
                                }
                            }
                        }
                    }

                    if (shouldSkip)
                    {
                        continue;
                    }
                }

                // if this is an unaltered component inside a nested prefab, skip it
                Transform? parentTransform = child.parent;
                GameObject currentChild = child.gameObject;
                while (parentTransform != null && parentTransform != initial)
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(parentTransform.gameObject))
                    {
                        shouldSkip = !PrefabUtility.IsAddedGameObjectOverride(currentChild);
                        List<ObjectOverride> overrides = PrefabUtility.GetObjectOverrides(parentTransform.gameObject, false);
                        foreach (ObjectOverride o in overrides)
                        {
                            UICompositeImage? comp = o.instanceObject as UICompositeImage;
                            if (comp != null && nestedComponents.AsValueEnumerable().Contains(comp))
                            {
                                shouldSkip = false;
                            }
                        }
                    }

                    if (shouldSkip)
                    {
                        break;
                    }

                    currentChild = parentTransform.gameObject;
                    parentTransform = parentTransform.parent;
                }

                if (shouldSkip)
                {
                    continue;
                }

                if (childComponents != null)
                {
                    foreach (var c in childComponents)
                    {
                        Log.Editor($"Linking {child.name} - component {c.gameObject.name}");
                    }
                }

                LinkRecursive(initial, child, childComponents);
            }
        }

        private static UICompositeImage[] GetNestedPrefabComponents(GameObject nestedPrefab)
        {
            return nestedPrefab.GetComponentsInChildren<UICompositeImage>(true);            
        }
        
        private static bool IsZeroRadius(Vector4 radius)
        {
            return Mathf.Approximately(radius.x, 0f) &&
                   Mathf.Approximately(radius.y, 0f) &&
                   Mathf.Approximately(radius.z, 0f) &&
                   Mathf.Approximately(radius.w, 0f);
        }

        private void Build(UICompositeImage original, List<string> builtSprites, string prefabPath)
        {
            Vector4 radius = SanitizeRadii(original.Radii);
            Log.Error($"{prefabPath} - radius: {radius.w} {radius.x} {radius.y} {radius.z}");
            // r=0 now represents hard 90° corners and maps to a minimal 1x1 texture
            bool isSquare = IsZeroRadius(radius);

            if (isSquare)
            {
                Debug.Log($"[Bake Info] {original.name} has zero radii — will route to r=0 sprite (true hard corners)", original);
            }

            // Skip baking if no filter layers and r=0; reuse shared r=0 sprite
            if (isSquare && original.GetComponents<UIBehaviour>().AsValueEnumerable().All(c => c.GetType().Name != "DropShadowFilter" &&
                                                                          c.GetType().Name != "OutlineFilter" &&
                                                                          c.GetType().Name != "GlowFilter"))
            {
                Debug.Log($"[Skip Info] {original.name} has no filter layers and zero radii. Using shared r=0 sprite.", original);
                if (_pendingLinks != null)
                {
                    string pixelName = GetSpriteName(original, UICompositeImageLayerType.Default);
                    string pngPath = Path.Combine(AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.ExportDirectory), pixelName);
                    _pendingLinks.Add((original, pngPath, UICompositeImageLayerType.Default));
                }
                return;
            }

            int width = CalculateWidth(radius);
            int height = CalculateHeight(radius);

            if (width <= 0 || height <= 0)
            {
                Debug.LogWarning($"[Auto Fix] {original.name} fallback triggered: Width={width} Height={height}", original);

                if (original.GetComponents<UIBehaviour>().AsValueEnumerable().All(c =>
                    c.GetType().Name != "DropShadowFilter" &&
                    c.GetType().Name != "OutlineFilter" &&
                    c.GetType().Name != "GlowFilter"))
                {
                    if (_pendingLinks != null)
                    {
                        string pixelName = GetSpriteName(original, UICompositeImageLayerType.Default);
                        string pngPath = Path.Combine(AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.ExportDirectory), pixelName);
                        _pendingLinks.Add((original, pngPath, UICompositeImageLayerType.Default));
                    }
                    return;
                }

                Log.Error(original, "[Build] Image {0} has invalid size (w:{1}, h:{2}).", AssetDatabase.GetAssetPath(original.gameObject), width, height);
                return;
            }

            int pad = Mathf.CeilToInt(original.FalloffDistance);
            width += pad * 2;
            height += pad * 2;

            // Enforce square dimensions for baking
            int size = Mathf.Max(width, height);
            width = size;
            height = size;

            foreach (UIDefaultImageLayer? layer in original.Layers)
            {
                string fileName = GetSpriteName(original, layer.LayerType);
                Log.Error($"fileName: {fileName}");
                if (builtSprites.Contains(fileName))
                {
                    Log.Error($"fileName: {fileName} - already handled");
                    // Already handled this sprite in this run — skip bake, only link
                    string existingPath = Path.Combine(AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.ExportDirectory), fileName);
                    _pendingLinks?.Add((original, existingPath, layer.LayerType));
                    continue;
                }

                (Camera cam, Canvas canvas) = SetupCaptureScene(width, height);
                UICompositeImage instance = SetupInstance(original, canvas, radius, width, height, layer.LayerType);

                HashSet<MonoBehaviour> toggledOff = new();
                EnableOnlyFilter(instance.gameObject, layer.LayerType, toggledOff);

                Texture2D? alphaTex = CaptureAlphaTexture(cam, width, height);
                if (alphaTex == null)
                {
                    Log.Error($"fileName: {fileName} - alphaTex is null");
                    continue;
                }

                string actualPngPath = SaveTextureAsPNG(alphaTex, fileName, instance);
                //string actualPngPath = SaveTextureAsPNGCropped(alphaTex, fileName, instance);
                // No sprite assignment here, just add to pendingLinks
                builtSprites.Add(fileName);
                if (_pendingLinks != null)
                {
                    _pendingLinks.Add((original, actualPngPath, layer.LayerType));
                }

                foreach (MonoBehaviour behaviour in toggledOff)
                {
                    SerializedObject so = new SerializedObject(behaviour);
                    SerializedProperty? enabledProp = so.FindProperty("m_Enabled");
                    if (enabledProp != null)
                    {
                        enabledProp.boolValue = false;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                CleanupScene(cam, canvas, instance.gameObject);
            }

            // No longer apply modifications or assign sprites here; linking is done in phase 2.
        }
        
        private static UICompositeImageLayerType? GetLayerType(System.Type t)
        {
            string typeName = t.Name;

            if (typeName == "DropShadowFilter")
            {
                return UICompositeImageLayerType.DropShadow;
            }

            if (typeName == "OutlineFilter")
            {
                return UICompositeImageLayerType.Outline;
            }

            if (typeName == "GlowFilter")
            {
                return UICompositeImageLayerType.Glow;
            }

            return null;
        }


        // ===============================
        // 🧠 Render Dimension Logic
        // -------------------------------
        // For r=1–7: use empirically tuned edge sizes to ensure clear rounded rendering
        // r=0 is not included here—it maps to 1x1 sharp corner texture and is handled elsewhere
        // For larger radii: scale using a 5% buffer to avoid GPU frustum edge clipping.
        // This ensures visual consistency, especially for rounded corners on small buttons.
        // ===============================
        // Empirical lookup: avoids aliasing at low radii (r=1+). r=0 bypasses this and uses pixel texture.
        private static int LookupEdge(int rA, int rB)
        {
            int sum = rA + rB;
            return sum switch
            {
                <= 2 => 3, // r=1 → safe 3px
                3 => 4,
                4 => 5,
                5 => 6,
                6 => 7,
                7 => 8,
                _ => -1 // fallback to proportional fudge
            };
        }

        // Fallback for larger radii: add 5% padding to ensure anti-aliasing and avoid clipping at corners.
        private static int ComputeEdge(int rA, int rB)
        {
            int lookup = LookupEdge(rA, rB);
            if (lookup > 0)
            {
                return lookup;
            }

            float raw = (rA + rB) * 1.05f; // 5% proportional padding
            return Mathf.Max(1, Mathf.CeilToInt(raw));
        }

        private static int CalculateWidth(Vector4 radius)
        {
            // Hybrid: lookup for small radii, proportional fudge for larger
            return Mathf.Max(
                ComputeEdge((int)radius.x, (int)radius.y),
                ComputeEdge((int)radius.x, (int)radius.z),
                ComputeEdge((int)radius.w, (int)radius.y),
                ComputeEdge((int)radius.w, (int)radius.z)
            );
        }

        private static int CalculateHeight(Vector4 radius)
        {
            // Hybrid: lookup for small radii, proportional fudge for larger
            return Mathf.Max(
                ComputeEdge((int)radius.x, (int)radius.w),
                ComputeEdge((int)radius.x, (int)radius.z),
                ComputeEdge((int)radius.y, (int)radius.w),
                ComputeEdge((int)radius.y, (int)radius.z)
            );
        }   

        private static (Camera, Canvas) SetupCaptureScene(int width, int height)
        {
            Camera cam = new GameObject($"{nameof(UICompositeImage)}.{nameof(Camera)}").AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.clear;
            cam.transform.position += new Vector3(0, 0, -10);
            cam.orthographicSize = height / 2f;

            Canvas canvas = new GameObject($"{nameof(UICompositeImage)}.{nameof(Canvas)}").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.pixelPerfect = false;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 |
                                                AdditionalCanvasShaderChannels.TexCoord2 |
                                                AdditionalCanvasShaderChannels.TexCoord3;

            return (cam, canvas);
        }

        private static UICompositeImage SetupInstance(UICompositeImage original, Canvas canvas, Vector4 radius, int width, int height, UICompositeImageLayerType layerType)
        {
            GameObject instance = new GameObject($"{nameof(UICompositeImage)}.{GetSpriteName(original, layerType)}", typeof(RectTransform));
            RectTransform rt = instance.GetComponent<RectTransform>();
            rt.SetParent(canvas.transform, false);

            rt.anchorMin = Vector2.one * 0.5f;
            rt.anchorMax = Vector2.one * 0.5f;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(width, height);

            if (width <= 0 || height <= 0)
            {
                Log.Error(original, "[SetupInstance] Invalid rect size for {0} (w:{1}, h:{2})", original.name, width, height);
            }
            instance.SetActive(true);
            canvas.gameObject.SetActive(true);
            Camera cam = canvas.worldCamera;
            if (cam != null)
            {
                cam.gameObject.SetActive(true);
            }

            UICompositeImage comp = instance.AddComponent<UICompositeImage>();
            comp.color = Color.white;
            comp.preserveAspect = false;
            comp.material = null;
            comp.Radii = radius;
            comp.Modifier = original.Modifier;
            comp.BorderWidth = original.BorderWidth;
            comp.FalloffDistance = original.FalloffDistance;
            comp.Mode = UICompositeImageRenderMode.Dynamic;
            comp.Update();
            
            return comp;
        }

        public Texture2D? CaptureAlphaTexture(Camera cam, int width, int height)
        {
            // 1) Validate inputs up front:
            if (cam == null)
            {
                Log.Error("CaptureAlphaTexture: Camera is null");
                return null;
            }
            if (width <= 0 || height <= 0)
            {
                Log.Error($"CaptureAlphaTexture: invalid dimensions {width}×{height}");
                return null;
            }
            int maxRT = SystemInfo.maxTextureSize;
            if (width > maxRT || height > maxRT)
            {
                Log.Error($"CaptureAlphaTexture: {width}×{height} exceeds system max {maxRT}");
                return null;
            }

            RenderTexture renderTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point
            };

            cam.targetTexture = renderTex;
            cam.Render();

            RenderTexture.active = renderTex;

            Texture2D fullTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            fullTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            // Insert skew warning if not square
            if (width != height)
            {
                Debug.LogWarning($"[Skew Warning] Captured render is not square: {width}x{height}");
            }
            fullTex.Apply();

            // Extract alpha
            Color32[] fullPixels = fullTex.GetPixels32();
            Color32[] alphaPixels = new Color32[fullPixels.Length];

            for (int i = 0; i < fullPixels.Length; i++)
            {
                byte r = fullPixels[i].r;
                alphaPixels[i] = new Color32(r, r, r, r);
            }

            Texture2D alphaTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            alphaTex.SetPixels32(alphaPixels);
            alphaTex.Apply();

            Object.DestroyImmediate(fullTex);
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(renderTex);

            return alphaTex;
        }

        public string SaveTextureAsPNG(Texture2D tex, string spriteName, UICompositeImage procImg)
        {
            string path = Path.Combine(AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.ExportDirectory), spriteName);
            byte[] newBytes = tex.EncodeToPNG();

            if (File.Exists(path) && _overwrittenThisRun.Contains(path))
            {
                // Skip re-write; already overwritten this run
                return path;
            }

            bool writeToPath = true;
            if (File.Exists(path))
            {
                byte[] existingBytes = File.ReadAllBytes(path);
                if (existingBytes.AsValueEnumerable().SequenceEqual(newBytes))
                {
                    writeToPath = false;
                }
            }

            if (writeToPath)
            {
                File.WriteAllBytes(path, newBytes);
                _overwrittenThisRun.Add(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            // Force border to match radii for perfect slicing
            TextureImporter? importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.isReadable = true;
                Vector4 radii = SanitizeRadii(procImg.Radii);
                Log.Error($"radii - {radii.w} {radii.z} {radii.y} {radii.x}");
                // Unity expects: left, bottom, right, top
                importer.spriteBorder = new Vector4(radii.w, radii.z, radii.y, radii.x);

                importer.SaveAndReimport();
            }

            return path;
        }

        public string SaveTextureAsPNGCropped(Texture2D tex, string spriteName, UICompositeImage procImg)
        {
            string path = Path.Combine(AssetDatabase.GetAssetPath(UICompositeImageSettings.Instance.ExportDirectory), spriteName);
            byte[] newBytes = tex.EncodeToPNG();

            if (File.Exists(path) && _overwrittenThisRun.Contains(path))
            {
                // Skip re-write; already overwritten this run
                return path;
            }

            if (File.Exists(path))
            {
                byte[] existingBytes = File.ReadAllBytes(path);
                if (existingBytes.AsValueEnumerable().SequenceEqual(newBytes))
                {
                    // No changes; skip write and preserve GUID
                    return path;
                }
            }

            File.WriteAllBytes(path, newBytes);
            _overwrittenThisRun.Add(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // Force border to match radii for perfect slicing
            TextureImporter? importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.isReadable = true;
                Vector4 radii = SanitizeRadii(procImg.Radii);
                // Unity expects: left, bottom, right, top
                importer.spriteBorder = new Vector4(radii.w, radii.z, radii.y, radii.x);

                importer.SaveAndReimport();
            }

            return path;
        }

        private static void CleanupScene(Camera cam, Canvas canvas, GameObject instance)
        {
            Object.DestroyImmediate(cam.gameObject);
            Object.DestroyImmediate(canvas.gameObject);
            Object.DestroyImmediate(instance);
        }

        public static string GetSpriteName(UICompositeImage procImg, UICompositeImageLayerType layerType)
        {
            Vector4 radius = SanitizeRadii(procImg.Radii);

            string radiusString;
            if (Mathf.Approximately(radius.x, radius.y) &&
                     Mathf.Approximately(radius.x, radius.z) &&
                     Mathf.Approximately(radius.x, radius.w))
            {
                radiusString = $"r{Mathf.RoundToInt(radius.x)}";
            }
            else
            {
                radiusString = $"r{Mathf.RoundToInt(radius.x)}_{Mathf.RoundToInt(radius.y)}_{Mathf.RoundToInt(radius.z)}_{Mathf.RoundToInt(radius.w)}";
            }

            string layerName = layerType == UICompositeImageLayerType.Default ? "" : $"_{layerType.ToString().ToLowerInvariant()}";
            return $"{UICompositeImageSettings.Instance.SpriteNamePrefix}_{(int)procImg.BorderWidth}_{(int)procImg.FalloffDistance}_{radiusString}{layerName}.png";
        }

        private static void EnableOnlyFilter(GameObject go, UICompositeImageLayerType type, HashSet<MonoBehaviour> toggledOff)
        {
            // Disable all GradientImage components
            var gradients = go.GetComponentsInChildren<MonoBehaviour>(true)
                .AsValueEnumerable()
                .Where(m => m.GetType().Name == "GradientImage" || m.GetType().Name == "Gradient2");
            foreach (MonoBehaviour? gradient in gradients)
            {
                SerializedObject so = new SerializedObject(gradient);
                SerializedProperty? enabledProp = so.FindProperty("m_Enabled");
                if (enabledProp != null)
                {
                    enabledProp.boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    toggledOff.Add(gradient);   
                }
            }

            // Force all FilterBase components to white
            var filters = go.GetComponentsInChildren<MonoBehaviour>(true)
                .AsValueEnumerable()
                .Where(m => m.GetType().Name.Contains("Filter"));
            foreach (MonoBehaviour? filter in filters)
            {
                SerializedObject soFilter = new SerializedObject(filter);
                SerializedProperty? colorProp = soFilter.FindProperty("color") ?? soFilter.FindProperty("_color") ?? soFilter.FindProperty("m_Color");
                if (colorProp != null)
                {
                    colorProp.colorValue = Color.white;
                    soFilter.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();

            if (type == UICompositeImageLayerType.Default)
            {
                foreach (MonoBehaviour component in components)
                {
                    string typeName = component.GetType().Name;
                    if (typeName == "DropShadowFilter" || typeName == "OutlineFilter" || typeName == "GlowFilter")
                    {
                        component.enabled = true;
                    }
                }
            }
            else
            {
                foreach (MonoBehaviour component in components)
                {
                    string typeName = component.GetType().Name;
                    UICompositeImageLayerType? filterType = GetLayerType(component.GetType());
                    if (typeName == "DropShadowFilter" || typeName == "OutlineFilter" || typeName == "GlowFilter")
                    {
                        component.enabled = (filterType == type);
                    }
                }
            }
        }
        
        // Ensures radii are never zero for sprite baking (avoids malformed sprites)
        private static Vector4 SanitizeRadii(Vector4 radius)
        {
            // Clamp minimum radius value to 1 for all corners to avoid invalid sprite generation.
            radius.x = Mathf.Max(radius.x, 1f);
            radius.y = Mathf.Max(radius.y, 1f);
            radius.z = Mathf.Max(radius.z, 1f);
            radius.w = Mathf.Max(radius.w, 1f);
            return radius;
        }

        // Prepares a UICompositeImage for effect-only export by disabling gradients and setting filter colors to white.
        public static void PrepareCompositeForEffectOnlyExport(UICompositeImage composite)
        {
            composite.Mode = UICompositeImageRenderMode.Dynamic;

            var gradients = composite.GetComponentsInChildren<MonoBehaviour>(true)
                .AsValueEnumerable()
                .Where(m => m.GetType().Name == "GradientImage" || m.GetType().Name == "Gradient2");
            foreach (MonoBehaviour? gradient in gradients)
            {
                SerializedObject so = new SerializedObject(gradient);
                SerializedProperty? enabledProp = so.FindProperty("m_Enabled");
                if (enabledProp != null)
                {
                    enabledProp.boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            var filterBases = composite.GetComponentsInChildren<MonoBehaviour>(true)
                .AsValueEnumerable()
                .Where(m => m.GetType().Name.Contains("Filter"));
            foreach (MonoBehaviour? filterBase in filterBases)
            {
                SerializedObject soFilter = new SerializedObject(filterBase);
                SerializedProperty? colorProp = soFilter.FindProperty("color") ?? soFilter.FindProperty("_color") ?? soFilter.FindProperty("m_Color");
                if (colorProp != null)
                {
                    colorProp.colorValue = Color.white;
                    soFilter.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
        
        
        /// <summary>
        /// Returns true if the given GameObject has any effect filter components (DropShadowFilter, OutlineFilter, GlowFilter).
        /// Uses type name string comparison to avoid compile-time dependencies.
        /// </summary>
        public static bool HasEffects(GameObject go)
        {
            if (go == null) return false;
            var components = go.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                string name = comp.GetType().Name;
                if (name == "DropShadowFilter" || name == "OutlineFilter" || name == "GlowFilter")
                    return true;
            }
            return false;
        }
    }
}