#nullable enable
using System.Collections.Generic;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using DeepForestLabs.Assets.Addressables;
using DeepForestLabs.PostProcessing;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeepForestLabs.AddressableManagement
{
    [InitializeOnLoad]
    public class AddressablesShaderPatcher
    {
        private static readonly Dictionary<string, Shader> _editorShaders = new();
        
        static AddressablesShaderPatcher()
        {
            if (_editorShaders.Count > 0)
            {
                return;
            }
            
            foreach (string guid in AssetDatabase.FindAssets("t:shader"))
            {
                Shader shader =
                    AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guid));
                _editorShaders[shader.name] = shader;
            }
            
            AddressablesManager.onLoadedScene += OnLoadedScene;
            AddressablesManager.onLoadedPrefab += OnLoadedPrefab;
            AddressablesManager.onLoadedPrefab += OnInstantiated;
        }

        private static void OnLoadedScene(Scene scene)
        {
            SetToEditorShader(scene);
        }

        private static void OnLoadedPrefab(GameObject prefab)
        {
            SetToEditorShader(prefab);
        }
        
        private static void OnInstantiated(GameObject instance)
        {
            //SetToEditorShader(instance);
        }

        protected static void SetToEditorShader(Scene scene)
        {
            foreach (GameObject? gameObject in scene.GetRootGameObjects())
            {
                SetToEditorShader(gameObject);
            }
        }

        private static void SetToEditorShader(GameObject gameObject)
        {
            if (BuildSettings.Instance.Addressables.ActivePlayModeIndex != BuilderIndex.AssetDatabasePlayMode)
            {
                foreach (Renderer? renderer in gameObject.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (Material? material in renderer.sharedMaterials)
                    {
                        if (material == null)
                        {
                            continue;
                        }

                        string shaderName = material.shader.name;
                        if (_editorShaders.TryGetValue(shaderName, out Shader? editorShader))
                        {
                            material.shader = editorShader;
                        }
                        else if ((editorShader = Shader.Find(shaderName)) != null)
                        {
                            material.shader = editorShader;
                            _editorShaders.Add(shaderName, editorShader);
                        }
                        else
                        {
                            Log.Warning("Failed to find editor shader for '{0}'.", shaderName);
                        }
                    }
                }

                foreach (Graphic? graphic in gameObject.GetComponentsInChildren<Graphic>(true))
                {
                    Material? material = graphic.material;
                    if (material == null)
                    {
                        continue;
                    }

                    string shaderName = material.shader.name;
                    if (_editorShaders.TryGetValue(shaderName, out Shader? editorShader))
                    {
                        material.shader = editorShader;
                    }
                    else if ((editorShader = Shader.Find(shaderName)) != null)
                    {
                        material.shader = editorShader;
                        _editorShaders.Add(shaderName, editorShader);
                    }
                    else
                    {
                        Log.Warning("Failed to find editor shader for '{0}'.", shaderName);
                    }
                }

                foreach (TMP_Text? text in gameObject.GetComponentsInChildren<TMP_Text>(true))
                {
                    List<Material?> materials = new() { text.materialForRendering, text.material };
                    foreach (Material? material in materials)
                    {
                        if (material == null)
                        {
                            continue;
                        }

                        string shaderName = material.shader.name;
                        if (_editorShaders.TryGetValue(shaderName, out Shader? editorShader))
                        {
                            material.shader = editorShader;
                        }
                        else if ((editorShader = Shader.Find(shaderName)) != null)
                        {
                            material.shader = editorShader;
                            _editorShaders.Add(shaderName, editorShader);
                        }
                        else
                        {
                            Log.Warning("Failed to find editor shader for '{0}'.", shaderName);
                        }
                    }
                }

                SetCustomEditorShaders(gameObject, _editorShaders);
            }
        }
        
        private static void SetCustomEditorShaders(GameObject gameObject, IDictionary<string, Shader> cachedShaders)
        {
            foreach (MobilePostProcessing? mobilePostProcessing in gameObject
                         .GetComponentsInChildren<MobilePostProcessing>(true))
            {
                Material? material = mobilePostProcessing.Material;
                if (material == null)
                {
                    continue;
                }

                string shaderName = material.shader.name;
                if (cachedShaders.TryGetValue(shaderName, out Shader? editorShader))
                {
                    material.shader = editorShader;
                }
                else if ((editorShader = Shader.Find(shaderName)) != null)
                {
                    material.shader = editorShader;
                    cachedShaders.Add(shaderName, editorShader);
                }
                else
                {
                    Log.Warning("Failed to find editor shader for '{0}'.", shaderName);
                }
            }
        }
    }
}
#nullable disable