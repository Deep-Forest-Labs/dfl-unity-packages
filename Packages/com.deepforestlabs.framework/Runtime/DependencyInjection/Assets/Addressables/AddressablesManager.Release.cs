#nullable enable
using System;
using System.Collections.Generic;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using AddressablesImpl = UnityEngine.AddressableAssets.Addressables;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager
    {
        private void ReleaseScene(SceneAssetHandle handle)
        {
            if (_sceneAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseScene(handle.OperationHandle).Forget();
            }
        }

        private void ReleaseAudioClip(AudioClipAssetHandle handle)
        {
            DelayedReleaseAudioClip(handle).Forget();
        }
        
        private void ReleaseMesh(MeshAssetHandle handle)
        {
            DelayedReleaseMesh(handle).Forget();
        }
        
        private void ReleaseRuntimeAnimatorController(RuntimeAnimatorControllerAssetHandle handle)
        {
            DelayedReleaseRuntimeAnimatorController(handle).Forget();
        }
        
        private void ReleaseSprite(SpriteAssetHandle handle)
        {
            DelayedReleaseSprite(handle).Forget();
        }
        
        private void ReleaseSpriteAtlas(SpriteAtlasAssetHandle handle)
        {
            DelayedReleaseSpriteAtlas(handle).Forget();
        }
        
        private void ReleaseTexture2D(Texture2DAssetHandle handle)
        {
            DelayedReleaseTexture2D(handle).Forget();
        }
        
        private void ReleaseScriptableObject(ScriptableObjectAssetHandle handle)
        {
            DelayedReleaseScriptableObject(handle).Forget();
        }
        
        private void ReleaseGameObject(GameObjectAssetHandle handle)
        {
            DelayedReleaseGameObject(handle).Forget();
        }
        
        private async UniTaskVoid DelayedReleaseAudioClip(AudioClipAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_audioClipAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseAudioClip(handle.OperationHandle);    
            }
        }

        private async UniTaskVoid DelayedReleaseMesh(MeshAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_meshAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseMesh(handle.OperationHandle);   
            }
        }
        
        private async UniTaskVoid DelayedReleaseRuntimeAnimatorController(RuntimeAnimatorControllerAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_runtimeAnimatorControllerAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseRuntimeAnimatorController(handle.OperationHandle);   
            }
        }
        
        private async UniTaskVoid DelayedReleaseSprite(SpriteAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_spriteAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseSprite(handle.OperationHandle);   
            }
        }
        
        private async UniTaskVoid DelayedReleaseSpriteAtlas(SpriteAtlasAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_spriteAtlasAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseSpriteAtlas(handle.OperationHandle);   
            }
        }
        
        private async UniTaskVoid DelayedReleaseTexture2D(Texture2DAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_texture2DAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseTexture2D(handle.OperationHandle);   
            }
        }
        
        private async UniTaskVoid DelayedReleaseScriptableObject(ScriptableObjectAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_scriptableObjectAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseScriptableObject(handle.OperationHandle);   
            }
        }
        
        private async UniTaskVoid DelayedReleaseGameObject(GameObjectAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, handle.GetContext(), "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }

            if (_gameObjectAssetHandles.Remove(handle.AssetReference))
            {
                SafeReleaseGameObject(handle.OperationHandle);   
            }
        }
        
        private static void SafeReleaseInitialize(AsyncOperationHandle<IResourceLocator> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseCheckForCatalogUpdates(AsyncOperationHandle<IResourceLocator> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseCatalogUpdate(AsyncOperationHandle<List<IResourceLocator>> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseDownload(AsyncOperationHandle handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseSize(AsyncOperationHandle<long> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static async UniTask SafeReleaseScene(AsyncOperationHandle<SceneInstance> handle)
        {
            if (!Application.isPlaying
#if UNITY_EDITOR
                || UnityEditor.EditorApplication.isCompiling
#endif
               )
            {
                return;
            }
            
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                Scene scene = handle.Result.Scene;
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    return;
                }

                if (!SceneManager.GetSceneByPath(scene.path).isLoaded)
                {
                    return;
                }

                await AddressablesImpl.UnloadSceneAsync(handle,
                        UnloadSceneOptions.UnloadAllEmbeddedSceneObjects)
                    .ToUniTask();
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to UnloadSceneAsync {0}.", handle.DebugName);
            }
            finally
            {
                try
                {
                    if (handle.IsValid())
                    {
                        AddressablesImpl.Release(handle);
                    }
                }
                catch (Exception e)
                {
                    Log.DevException(e, "Failed to release {0}.", handle.DebugName);
                }
            }
        }
        
        private static void SafeReleaseAudioClip(AsyncOperationHandle<AudioClip> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseMesh(AsyncOperationHandle<Mesh> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseRuntimeAnimatorController(AsyncOperationHandle<RuntimeAnimatorController> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseSprite(AsyncOperationHandle<Sprite> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseSpriteAtlas(AsyncOperationHandle<SpriteAtlas> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }

                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseTexture2D(AsyncOperationHandle<Texture2D> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }

                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseScriptableObject(AsyncOperationHandle<ScriptableObject> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }

                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
        
        private static void SafeReleaseGameObject(AsyncOperationHandle<GameObject> handle)
        {
            try
            {
                if (!handle.IsValid())
                {
                    return;
                }
                
                AddressablesImpl.Release(handle);
            }
            catch (Exception e)
            {
                Log.DevException(e, "Failed to release {0}.", handle.DebugName);
            }
        }
    }
}
#nullable disable