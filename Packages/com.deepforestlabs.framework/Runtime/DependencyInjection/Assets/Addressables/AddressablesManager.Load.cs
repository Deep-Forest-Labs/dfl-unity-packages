#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using AddressablesImpl = UnityEngine.AddressableAssets.Addressables;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager
    {
        public async UniTask<Scene> LoadScene(string guid, CancellationToken token)
        {
            AssetReference assetReference = GetSceneAssetReference(guid);

            if (!_sceneAssetHandles.TryGetValue(assetReference, out SceneAssetHandle? assetHandle))
            {
                SceneDownloadHandle downloadHandle = GetSceneDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(location.ResourceType == typeof(SceneInstance) || 
                           location.InternalId.EndsWith(".unity", StringComparison.OrdinalIgnoreCase), 
                        downloadHandle.EditorContext,
                        "Location for {0} does not look like a scene. Provider={1}, Id={2}",
                        downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                SceneLoadHandle loadHandle = GetSceneLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
                
                Log.Assert(_sceneAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_sceneAssetHandles.ContainsKey({0})", assetReference);
                assetHandle = _sceneAssetHandles[assetReference];
            }

            assetHandle.Push(token);
            return assetHandle.Asset.Scene;
        }

        public async UniTask<AudioClip> LoadAudioClip(string guid, CancellationToken token)
        {
            AssetReferenceT<AudioClip> assetReference = GetAudioClipAssetReference(guid);

            if (!_audioClipAssetHandles.TryGetValue(assetReference, out AudioClipAssetHandle? assetHandle))
            {
                AudioClipDownloadHandle downloadHandle = GetAudioClipDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(location.ResourceType == typeof(AudioClip),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an AudioClip. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                AudioClipLoadHandle loadHandle = GetAudioClipLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
            
                Log.Assert(_audioClipAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_audioClipAssetHandle.ContainsKey({0})", assetReference);
                assetHandle = _audioClipAssetHandles[assetReference];
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }
        
        public async UniTask<Mesh> LoadMesh(string guid, CancellationToken token)
        {
            AssetReferenceT<Mesh> assetReference = GetMeshAssetReference(guid);

            if (!_meshAssetHandles.TryGetValue(assetReference, out MeshAssetHandle? assetHandle))
            {
                MeshDownloadHandle downloadHandle = GetMeshDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(location.ResourceType == typeof(Mesh),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an Mesh. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                MeshLoadHandle loadHandle = GetMeshLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
            
                Log.Assert(_meshAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_meshAssetHandle.ContainsKey({0})", assetReference);
                assetHandle = _meshAssetHandles[assetReference];
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }
        
        public async UniTask<RuntimeAnimatorController> LoadRuntimeAnimatorController(string guid, CancellationToken token)
        {
            AssetReferenceT<RuntimeAnimatorController> assetReference = GetRuntimeAnimatorControllerAssetReference(guid);

            if (!_runtimeAnimatorControllerAssetHandles.TryGetValue(assetReference, out RuntimeAnimatorControllerAssetHandle? assetHandle))
            {
                RuntimeAnimatorControllerDownloadHandle downloadHandle = GetRuntimeAnimatorControllerDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(typeof(RuntimeAnimatorController).IsAssignableFrom(location.ResourceType),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an RuntimeAnimatorController. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                RuntimeAnimatorControllerLoadHandle loadHandle = GetRuntimeAnimatorControllerLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
            
                Log.Assert(_runtimeAnimatorControllerAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_runtimeAnimatorControllerAssetHandle.ContainsKey({0})", assetReference);
                assetHandle = _runtimeAnimatorControllerAssetHandles[assetReference];
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }
        
        public async UniTask<Sprite> LoadSprite(string guid, CancellationToken token)
        {
            AssetReferenceSprite assetReference = GetSpriteAssetReference(guid);

            if (!_spriteAssetHandles.TryGetValue(assetReference, out SpriteAssetHandle? assetHandle))
            {
                SpriteDownloadHandle downloadHandle = GetSpriteDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(location.ResourceType == typeof(Sprite),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an Sprite. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                SpriteLoadHandle loadHandle = GetSpriteLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
            
                Log.Assert(_spriteAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_spriteAssetHandle.ContainsKey({0})", assetReference);
                assetHandle = _spriteAssetHandles[assetReference];
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }
        
        public async UniTask<Sprite> LoadAtlasedSprite(string guid, string spriteName, CancellationToken token)
        {
            SpriteAtlas spriteAtlas = await LoadSpriteAtlas(guid, token);
            Sprite? sprite = spriteAtlas.GetSprite(spriteName);
            
            Log.Assert(sprite != null, spriteAtlas, "Sprite atlas does not contain sprite named '{0}'.", sprite);

            return sprite;
        }
        
        public async UniTask<SpriteAtlas> LoadSpriteAtlas(string guid, CancellationToken token)
        {
            AssetReferenceT<SpriteAtlas> assetReference = GetSpriteAtlasAssetReference(guid);

            if (!_spriteAtlasAssetHandles.TryGetValue(assetReference, out SpriteAtlasAssetHandle? assetHandle))
            {
                SpriteAtlasDownloadHandle downloadHandle = GetSpriteAtlasDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(location.ResourceType == typeof(SpriteAtlas),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an SpriteAtlas. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                SpriteAtlasLoadHandle loadHandle = GetSpriteAtlasLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
                
                Log.Assert(_spriteAtlasAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_spriteAtlasAssetHandles.ContainsKey({0})", assetReference);
                assetHandle = _spriteAtlasAssetHandles[assetReference];
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }
        
        public async UniTask<Texture2D> LoadTexture2D(string guid, CancellationToken token)
        {
            AssetReferenceTexture2D assetReference = GetTexture2DAssetReference(guid);

            if (!_texture2DAssetHandles.TryGetValue(assetReference, out Texture2DAssetHandle? assetHandle))
            {
                Texture2DDownloadHandle downloadHandle = GetTexture2DDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(location.ResourceType == typeof(Texture2D),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an Texture2D. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                Texture2DLoadHandle loadHandle = GetTexture2DLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
            
                Log.Assert(_texture2DAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_texture2DAssetHandle.ContainsKey({0})", assetReference);
                assetHandle = _texture2DAssetHandles[assetReference];
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }

        public async UniTask<ScriptableObject> LoadScriptableObject(string guid, CancellationToken token)
        {
            AssetReferenceT<ScriptableObject> assetReference = GetScriptableObjectAssetReference(guid);

            if (!_scriptableObjectAssetHandles.TryGetValue(assetReference, out ScriptableObjectAssetHandle? assetHandle))
            {
                ScriptableObjectDownloadHandle downloadHandle = GetScriptableObjectDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(typeof(ScriptableObject).IsAssignableFrom(location.ResourceType),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an ScriptableObject. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                ScriptableObjectLoadHandle loadHandle = GetScriptableObjectLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);

                Log.Assert(_scriptableObjectAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_scriptableObjectAssetHandle.ContainsKey({0})", assetReference);
                assetHandle = _scriptableObjectAssetHandles[assetReference];
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }

        public async UniTask<GameObject> LoadGameObject(string guid, CancellationToken token)
        {
            AssetReferenceGameObject assetReference = GetGameObjectAssetReference(guid);

            if (!_gameObjectAssetHandles.TryGetValue(assetReference, out GameObjectAssetHandle? assetHandle))
            {
                GameObjectDownloadHandle downloadHandle = GetGameObjectDownloadHandle(guid, assetReference);
                IReadOnlyList<IResourceLocation> locations = await downloadHandle
                    .WaitUntilCached(token);
                
                Log.Assert(locations.Count == 1, "locations.Count == 1");
                IResourceLocation location = locations[0];
                Log.Assert(location.ResourceType == typeof(GameObject),
                    downloadHandle.EditorContext,
                    "Location for {0} does not look like an GameObject. Provider={1}, Id={2}",
                    downloadHandle.Guid, location.ProviderId, location.InternalId);
                
                GameObjectLoadHandle loadHandle = GetGameObjectLoadHandle(location, downloadHandle);
                await loadHandle.WaitUntilLoaded(token);
            
                Log.Assert(_gameObjectAssetHandles.ContainsKey(assetReference), downloadHandle.EditorContext, "_gameObjectAssetHandles.ContainsKey({0})", assetReference);
                assetHandle = _gameObjectAssetHandles[assetReference];
                
#if UNITY_EDITOR
                onLoadedPrefab?.Invoke(assetHandle.Asset);
#endif
            }
            
            assetHandle.Push(token);
            return assetHandle.Asset;
        }
        
        public async UniTask<GameObjectManager> LoadGameObjectManager(GameObjectAssetRef assetRef,
            GameObjectManagerOptions options, CancellationToken token)
        {
            AssetReferenceGameObject assetReference = GetGameObjectAssetReference(assetRef._guid);
            
            if (!_gameObjectManagers.TryGetValue(assetReference, out GameObjectManager? manager))
            {
                manager = new GameObjectManager(_container, assetRef, options);
                _gameObjectManagers.Add(assetReference, manager);
            }
            manager.Push(token);
            
            await manager.LoadRequired(_scope)
                .AttachExternalCancellation(token);
            
            return manager;
        }

        public async UniTask<GameObjectManagerT<T>> LoadGameObjectManager<T>(GameObjectAssetRefT<T> assetRef,
            GameObjectManagerOptions options, CancellationToken token)
            where T : Component
        {
            AssetReferenceGameObject assetReference = GetGameObjectAssetReference(assetRef._guid);
            
            if (!_typedGameObjectManagers.TryGetValue(assetReference, out Dictionary<Type, UniTaskCompletionSource<object>>? managers))
            {
                managers = new Dictionary<Type, UniTaskCompletionSource<object>>();
                _typedGameObjectManagers.Add(assetReference, managers);
            }

            GameObjectManagerT<T> manager;
            if (!managers.TryGetValue(typeof(T), out UniTaskCompletionSource<object>? taskCompletionSource))
            {
                taskCompletionSource = new UniTaskCompletionSource<object>();
                managers.Add(typeof(T), taskCompletionSource);
                GameObjectManager proxy = await LoadGameObjectManager((GameObjectAssetRef)assetRef, options, token);
                manager = new GameObjectManagerT<T>(proxy);
                
                taskCompletionSource.TrySetResult(manager);
            }
            else
            {
                object result = await taskCompletionSource.Task;
                Log.Assert(result is GameObjectManagerT<T>, "obj is GameObjectManagerT<T>");
                manager = (result as GameObjectManagerT<T>)!;
            }
            
            manager.Push(token);
            
            return manager;
        }
        
        private SceneLoadHandle GetSceneLoadHandle(IResourceLocation location, SceneDownloadHandle downloadHandle)
        {
            if (!_scenesLoadHandles.TryGetValue(location,
                    out SceneLoadHandle? loadHandle))
            {
                loadHandle = new SceneLoadHandle(this, downloadHandle, location);
                _scenesLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private AudioClipLoadHandle GetAudioClipLoadHandle(IResourceLocation location, AudioClipDownloadHandle downloadHandle)
        {
            if (!_audioClipLoadHandles.TryGetValue(location,
                    out AudioClipLoadHandle? loadHandle))
            {
                loadHandle = new AudioClipLoadHandle(this, downloadHandle, location);
                _audioClipLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private MeshLoadHandle GetMeshLoadHandle(IResourceLocation location, MeshDownloadHandle downloadHandle)
        {
            if (!_meshLoadHandles.TryGetValue(location,
                    out MeshLoadHandle? loadHandle))
            {
                loadHandle = new MeshLoadHandle(this, downloadHandle, location);
                _meshLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private RuntimeAnimatorControllerLoadHandle GetRuntimeAnimatorControllerLoadHandle(IResourceLocation location, RuntimeAnimatorControllerDownloadHandle downloadHandle)
        {
            if (!_runtimeAnimatorControllerLoadHandles.TryGetValue(location,
                    out RuntimeAnimatorControllerLoadHandle? loadHandle))
            {
                loadHandle = new RuntimeAnimatorControllerLoadHandle(this, downloadHandle, location);
                _runtimeAnimatorControllerLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private SpriteLoadHandle GetSpriteLoadHandle(IResourceLocation location, SpriteDownloadHandle downloadHandle)
        {
            if (!_spriteLoadHandles.TryGetValue(location,
                    out SpriteLoadHandle? loadHandle))
            {
                loadHandle = new SpriteLoadHandle(this, downloadHandle, location);
                _spriteLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private SpriteAtlasLoadHandle GetSpriteAtlasLoadHandle(IResourceLocation location, SpriteAtlasDownloadHandle downloadHandle)
        {
            if (!_spriteAtlasLoadHandles.TryGetValue(location,
                    out SpriteAtlasLoadHandle? loadHandle))
            {
                loadHandle = new SpriteAtlasLoadHandle(this, downloadHandle, location);
                _spriteAtlasLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private Texture2DLoadHandle GetTexture2DLoadHandle(IResourceLocation location, Texture2DDownloadHandle downloadHandle)
        {
            if (!_texture2DLoadHandles.TryGetValue(location,
                    out Texture2DLoadHandle? loadHandle))
            {
                loadHandle = new Texture2DLoadHandle(this, downloadHandle, location);
                _texture2DLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private ScriptableObjectLoadHandle GetScriptableObjectLoadHandle(IResourceLocation location, ScriptableObjectDownloadHandle downloadHandle)
        {
            if (!_scriptableObjectLoadHandles.TryGetValue(location,
                    out ScriptableObjectLoadHandle? loadHandle))
            {
                loadHandle = new ScriptableObjectLoadHandle(this, downloadHandle, location);
                _scriptableObjectLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private GameObjectLoadHandle GetGameObjectLoadHandle(IResourceLocation location, GameObjectDownloadHandle downloadHandle)
        {
            if (!_gameObjectLoadHandles.TryGetValue(location,
                    out GameObjectLoadHandle? loadHandle))
            {
                loadHandle = new GameObjectLoadHandle(this, downloadHandle, location);
                _gameObjectLoadHandles.Add(location, loadHandle);
            }

            return loadHandle;
        }
        
        private async UniTaskVoid LoadSceneInBackground(SceneLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<SceneInstance> operation =
                    AddressablesImpl.LoadSceneAsync(loadHandle.Location, LoadSceneMode.Additive, activateOnLoad:false);
                try
                {
                    SceneInstance result = await operation.ToUniTask(cancellationToken: token);
                    if (!_scenesLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        await SafeReleaseScene(operation);
                        return;
                    }
                    
                    SceneAssetHandle assetHandle = new SceneAssetHandle(this, loadHandle, operation);
                    _sceneAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    await result.ActivateAsync();
#if UNITY_EDITOR
                    onLoadedScene?.Invoke(result.Scene);
#endif
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    await SafeReleaseScene(operation);
                    throw;
                }
                catch (Exception e)
                {
                    ex = PreferOperationException(operation, e);
                    await SafeReleaseScene(operation);
                    token.ThrowIfCancellationRequested();
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadScene(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadScene(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private async UniTaskVoid LoadAudioClipInBackground(AudioClipLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<AudioClip> operation =
                    AddressablesImpl.LoadAssetAsync<AudioClip>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_audioClipLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseAudioClip(operation);
                        return;
                    }

                    AudioClipAssetHandle assetHandle = new AudioClipAssetHandle(this, loadHandle, operation);
                    _audioClipAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseAudioClip(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseAudioClip(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadAudioClip(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadAudioClip(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private async UniTaskVoid LoadMeshInBackground(MeshLoadHandle loadHandle , CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<Mesh> operation = AddressablesImpl.LoadAssetAsync<Mesh>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_meshLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseMesh(operation);
                        return;
                    }
                    
                    MeshAssetHandle assetHandle = new MeshAssetHandle(this, loadHandle, operation);
                    _meshAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseMesh(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseMesh(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadMesh(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadMesh(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private async UniTaskVoid LoadRuntimeAnimatorControllerInBackground(RuntimeAnimatorControllerLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<RuntimeAnimatorController> operation = AddressablesImpl.LoadAssetAsync<RuntimeAnimatorController>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_runtimeAnimatorControllerLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseRuntimeAnimatorController(operation);
                        return;
                    }
                    
                    RuntimeAnimatorControllerAssetHandle assetHandle = new RuntimeAnimatorControllerAssetHandle(this, loadHandle, operation);
                    _runtimeAnimatorControllerAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseRuntimeAnimatorController(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseRuntimeAnimatorController(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadRuntimeAnimatorController(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadRuntimeAnimatorController(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private async UniTaskVoid LoadSpriteInBackground(SpriteLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<Sprite> operation = AddressablesImpl.LoadAssetAsync<Sprite>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_spriteLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseSprite(operation);
                        return;
                    }
                    
                    SpriteAssetHandle assetHandle = new SpriteAssetHandle(this, loadHandle, operation);
                    _spriteAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseSprite(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseSprite(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadSprite(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadSprite(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private async UniTaskVoid LoadSpriteAtlasInBackground(SpriteAtlasLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<SpriteAtlas> operation =
                    AddressablesImpl.LoadAssetAsync<SpriteAtlas>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_spriteAtlasLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseSpriteAtlas(operation);
                        return;
                    }
                    
                    SpriteAtlasAssetHandle assetHandle = new SpriteAtlasAssetHandle(this, loadHandle, operation);
                    _spriteAtlasAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseSpriteAtlas(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseSpriteAtlas(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadSpriteAtlas(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadSpriteAtlas(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private async UniTaskVoid LoadTexture2DInBackground(Texture2DLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<Texture2D> operation =
                    AddressablesImpl.LoadAssetAsync<Texture2D>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_texture2DLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseTexture2D(operation);
                        return;
                    }
                    
                    Texture2DAssetHandle assetHandle = new Texture2DAssetHandle(this, loadHandle, operation);
                    _texture2DAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseTexture2D(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseTexture2D(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadTexture2D(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadTexture2D(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
        
        private async UniTaskVoid LoadScriptableObjectInBackground(ScriptableObjectLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<ScriptableObject> operation =
                    AddressablesImpl.LoadAssetAsync<ScriptableObject>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_scriptableObjectLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseScriptableObject(operation);
                        return;
                    }
                    
                    ScriptableObjectAssetHandle assetHandle =
                        new ScriptableObjectAssetHandle(this, loadHandle, operation);
                    _scriptableObjectAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseScriptableObject(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseScriptableObject(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    
                    await DownloadScriptableObject(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadScriptableObject(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
      
        private async UniTaskVoid LoadGameObjectInBackground(GameObjectLoadHandle loadHandle, CancellationToken token)
        {
            while (true)
            {
                Exception ex;
                AsyncOperationHandle<GameObject> operation =
                    AddressablesImpl.LoadAssetAsync<GameObject>(loadHandle.Location);
                try
                {
                    await operation.ToUniTask(cancellationToken: token);
                    if (!_gameObjectLoadHandles.TryGetValue(loadHandle.Location, out _)
                        || token.IsCancellationRequested 
                        || loadHandle.ReferenceCount == 0)
                    {
                        SafeReleaseGameObject(operation);
                        return;
                    }
                    
                    GameObjectAssetHandle assetHandle = new GameObjectAssetHandle(this, loadHandle, operation);
                    _gameObjectAssetHandles.Add(loadHandle.DownloadHandle.AssetReference, assetHandle);
                    loadHandle.OnLoadComplete(operation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    SafeReleaseGameObject(operation);
                    throw;
                }
                catch (Exception e)
                {
                    SafeReleaseGameObject(operation);
                    ex = PreferOperationException(operation, e);
                }
                
                if (IsConfigError(ex))
                {
                    throw ex;
                }

                if (IsTransient(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Check network.",
                        loadHandle.Guid);
                    await DownloadGameObject(loadHandle.Guid, token);
                    continue;
                }
                
                if (LooksLikeCorruption(ex))
                {
                    Log.DebugException(loadHandle.GetContext(), ex, "Load ['{0}'] batch was not successful. Looks like corrupted bundle(s).",
                        loadHandle.Guid);
                    
                    AsyncOperationHandle<bool> clearOperation = AddressablesImpl.ClearDependencyCacheAsync(loadHandle.AssetReference, autoReleaseHandle: true);
                    await clearOperation.ToUniTask();
                    await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, token);

                    await DownloadGameObject(loadHandle.Guid, token);
                    continue;
                }
                
                // Unclassified, should not happen
                throw ex;
            }
        }
    }
}
#nullable disable