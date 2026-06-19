#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.U2D;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager
    {
        public IEnumerable<IResourceLocation> GetSceneLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetSceneAssetReference(guid), typeof(SceneInstance)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetAudioClipLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetAudioClipAssetReference(guid), typeof(AudioClip)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetMeshLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetMeshAssetReference(guid), typeof(Mesh)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetRuntimeAnimatorControllerLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetRuntimeAnimatorControllerAssetReference(guid), typeof(RuntimeAnimatorController)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetSpriteLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetSpriteAssetReference(guid), typeof(Sprite)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetSpriteAtlasLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetSpriteAtlasAssetReference(guid), typeof(SpriteAtlas)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetTexture2DLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetTexture2DAssetReference(guid), typeof(Texture2D)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetScriptableObjectLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetScriptableObjectAssetReference(guid), typeof(ScriptableObject)))
            {
                yield return location;
            }
        }
        
        public IEnumerable<IResourceLocation> GetGameObjectLocations(string guid)
        {
            foreach (IResourceLocation location in GetLocations(GetGameObjectAssetReference(guid), typeof(GameObject)))
            {
                yield return location;
            }
        }
        
        private IEnumerable<IResourceLocation> GetLocations(AssetReference assetReference, Type assetType)
        {
            HashSet<IResourceLocation> set = new HashSet<IResourceLocation>(ResourceLocationComparer.Instance);
            foreach (IResourceLocator? loc in _locators)
            {
                if (loc.Locate(assetReference.RuntimeKey, assetType, out IList<IResourceLocation>? locations) && locations != null)
                {
                    foreach (IResourceLocation location in locations)
                    {
                        if (set.Add(location))
                        {
                            yield return location;    
                        }
                    }
                }
            }
        }
    }
}