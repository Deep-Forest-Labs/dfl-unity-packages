#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using ZLinq;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Assets.Resource;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace DeepForestLabs
{
    internal sealed partial class Container
    {
        public bool CanLocate(SceneAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
            
        public bool CanLocate(AudioClipAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public bool CanLocate(MeshAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public bool CanLocate(SpriteAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public bool CanLocate(AtlasedSpriteAssetRef assetRef)
        {
            //TODO - add a check that the location points to a sprite atlas with the sprite in it.
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public bool CanLocate(Texture2DAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public bool CanLocate(SpriteAtlasAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public bool CanLocate(ScriptableObjectAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public bool CanLocate(GameObjectAssetRef assetRef)
        {
            return GetLocations(assetRef).AsValueEnumerable().Any();
        }
        
        public IEnumerable<IResourceLocation> GetLocations(SceneAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(Scene), assetRef);
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(Scene));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetSceneLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }
        
        public IEnumerable<IResourceLocation> GetLocations(AudioClipAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(AudioClip), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(AudioClip));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetAudioClipLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }
        
        public IEnumerable<IResourceLocation> GetLocations(MeshAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(Mesh), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(Mesh));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetMeshLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<IResourceLocation> GetLocations(SpriteAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(Sprite), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(Sprite));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetSpriteLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<IResourceLocation> GetLocations(SpriteAtlasAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(Sprite), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(SpriteAtlas));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetSpriteAtlasLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<IResourceLocation> GetLocations(Texture2DAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(Texture2D), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(Texture2D));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetTexture2DLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<IResourceLocation> GetLocations(ScriptableObjectAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(ScriptableObject), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(ScriptableObject));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetScriptableObjectLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<IResourceLocation> GetLocations(GameObjectAssetRef assetRef)
        {
            Log.Assert(assetRef.IsValid(), "Invalid {0} '{1}'", nameof(GameObjectAssetRef), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    yield return new ResourcesLocation(assetRef._resourcesPath, typeof(Texture2D));
                    yield break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    foreach (IResourceLocation location in _addressablesManager.GetGameObjectLocations(assetRef._guid))
                    {
                        yield return location;
                    }
                    yield break;
                default:
                    throw new NotSupportedException();
            }    
        }

        public UniTask Download(SceneAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Scene), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadScene(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }

        public UniTask Download(AudioClipAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(AudioClip), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadAudioClip(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }

        public UniTask Download(MeshAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Mesh), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadMesh(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }

        public UniTask Download(SpriteAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Sprite), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadSprite(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask Download(AtlasedSpriteAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Sprite), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadAtlasedSprite(assetRef._guid, assetRef._spriteName, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask Download(SpriteAtlasAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Sprite), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadSpriteAtlas(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask Download(Texture2DAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Texture2D), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadTexture2D(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask Download(ScriptableObjectAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(ScriptableObject), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadScriptableObject(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask Download(GameObjectAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(GameObject), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return UniTask.CompletedTask;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.DownloadGameObject(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask<Scene> Checkout(SceneAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Scene), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return _resourcesManager.LoadScene(assetRef._resourcesPath, token);
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.LoadScene(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask<AudioClip> Checkout(AudioClipAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(AudioClip), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return _resourcesManager.LoadAudioClip(assetRef._resourcesPath, token);
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.LoadAudioClip(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public UniTask<Mesh> Checkout(MeshAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Mesh), assetRef);
            
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    return _resourcesManager.LoadMesh(assetRef._resourcesPath, token);
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    return _addressablesManager.LoadMesh(assetRef._guid, token);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public async UniTask<Sprite> Checkout(SpriteAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Sprite), assetRef);
            
            Sprite result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadSprite(assetRef._resourcesPath, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadSprite(assetRef._guid, token);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
        
        public async UniTask<Sprite> Checkout(SpriteAssetRef assetRef, Image? image,
            CancellationToken token)
        {
            Sprite result = await Checkout(assetRef, token);
            if (image != null)
            {
                image.sprite = result;
            }

            return result;
        }
        
        public async UniTask<Sprite> Checkout(SpriteAssetRef assetRef, SpriteRenderer renderer,
            CancellationToken token)
        {
            Sprite result = await Checkout(assetRef, token);
            if (renderer != null)
            {
                renderer.sprite = result;
            }

            return result;
        }
        
        public async UniTask<Sprite> Checkout(AtlasedSpriteAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Sprite), assetRef);
            
            Sprite result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadAtlasedSprite(assetRef._resourcesPath, assetRef._spriteName, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadAtlasedSprite(assetRef._guid, assetRef._spriteName, token);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
        
        public async UniTask<Sprite> Checkout(AtlasedSpriteAssetRef assetRef,
            Image? image, CancellationToken token)
        {
            Sprite result = await Checkout(assetRef, token);
            if (image != null)
            {
                image.sprite = result;
            }

            return result;
        }
        
        public async UniTask<SpriteAtlas> Checkout(SpriteAtlasAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Sprite), assetRef);
            
            SpriteAtlas result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadSpriteAtlas(assetRef._resourcesPath, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadSpriteAtlas(assetRef._guid, token);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        public async UniTask<Sprite> Checkout(AtlasedSpriteAssetRef assetRef, SpriteRenderer spriteRenderer, CancellationToken token)
        {
            Sprite result = await Checkout(assetRef, token);
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = result;
            }

            return result;
        }
        
        public async UniTask<Texture2D> Checkout(Texture2DAssetRef assetRef, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(Texture2D), assetRef);
            
            Texture2D result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadTexture2D(assetRef._resourcesPath, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadTexture2D(assetRef._guid, token);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }

        public async UniTask<Texture2D> Checkout(Texture2DAssetRef assetRef, RawImage? image,
            CancellationToken token)
        {
            Texture2D result = await Checkout(assetRef, token);
            if (image != null)
            {
                image.texture = result;
            }

            return result;
        }

        public async UniTask<T> Checkout<T>(ScriptableObjectAssetRefT<T> assetRef, CancellationToken token) where T : ScriptableObject
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", typeof(T).Name, assetRef);
            
            ScriptableObject result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadScriptableObject(assetRef._resourcesPath, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadScriptableObject(assetRef._guid, token);
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (result is not T instance)
            {
                throw GameException.FromFormat("Invalid cast from '{0}' to '{1}'.", nameof(ScriptableObject), typeof(T).Name);
            }

            return instance;
        }

        public async UniTask<IGameObjectManager> Checkout(GameObjectAssetRef assetRef, GameObjectManagerOptions options, CancellationToken token)
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", nameof(GameObject), assetRef);
            
            GameObjectManager result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadGameObjectManager(assetRef, options, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadGameObjectManager(assetRef, options, token);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }

        [Obsolete("Use GameObjectAssetRefT<T> instead")]
        public UniTask<IGameObjectManagerT<T>> Checkout<T>(GameObjectAssetRef? assetRef,
            GameObjectManagerOptions options, CancellationToken token) where T : Component
        {
            throw new NotSupportedException();
        }

        public async UniTask<IGameObjectManagerT<T>> Checkout<T>(GameObjectAssetRefT<T> assetRef,
            GameObjectManagerOptions options, CancellationToken token) where T : Component
        {
            Log.Assert(CanLocate(assetRef), "Failed to located {0} '{1}'.", typeof(T).Name, assetRef);
            
            GameObjectManagerT<T> result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadGameObjectManager(assetRef, options, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadGameObjectManager(assetRef, options, token);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
        
        internal async UniTask<GameObject> CheckoutPrefab(GameObjectAssetRef assetRef, CancellationToken token)
        {
            GameObject result;
            switch (assetRef._mode)
            {
                case AssetRefMode.Resources:
                    result = await _resourcesManager.LoadGameObject(assetRef._resourcesPath, token);
                    break;
                case AssetRefMode.Addressables:
                    Log.Assert(_addressablesManager != null, nameof(_addressablesManager) + " != null");
                    result = await _addressablesManager.LoadGameObject(assetRef._guid, token);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
    }
}
#nullable disable