#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Assets.Resource
{
    internal sealed partial class ResourcesManager
    {
        private readonly Container _container;
        private readonly CancellationToken _scope;

        private readonly Dictionary<string, SceneAssetHandle> _scenes = new();
        private readonly Dictionary<string, AudioClipAssetHandle> _audioClips = new();
        private readonly Dictionary<string, MeshAssetHandle> _meshes = new();
        private readonly Dictionary<string, SpriteAssetHandle> _sprites = new();
        private readonly Dictionary<string, SpriteAtlasAssetHandle> _spriteAtlases = new();
        private readonly Dictionary<string, Texture2DAssetHandle> _textures = new();
        private readonly Dictionary<string, ScriptableObjectAssetHandle> _scriptableObjects = new();
        private readonly Dictionary<string, GameObjectAssetHandle> _gameObject = new();
        private readonly Dictionary<string, GameObjectManager> _gameObjectManagers = new();
        private readonly Dictionary<string, Dictionary<Type, UniTaskCompletionSource<object>>> _typedGameObjectManagers = new();

        private readonly Dictionary<string, UniTaskCompletionSource<AudioClipAssetHandle>> _pendingAudioClips = new();
        private readonly Dictionary<string, UniTaskCompletionSource<MeshAssetHandle>> _pendingMeshes = new();
        private readonly Dictionary<string, UniTaskCompletionSource<SpriteAssetHandle>> _pendingSprites = new();
        private readonly Dictionary<string, UniTaskCompletionSource<SpriteAtlasAssetHandle>> _pendingSpriteAtlases = new();
        private readonly Dictionary<string, UniTaskCompletionSource<Texture2DAssetHandle>> _pendingTextures = new();
        private readonly Dictionary<string, UniTaskCompletionSource<ScriptableObjectAssetHandle>> _pendingScriptableObjects = new();
        private readonly Dictionary<string, UniTaskCompletionSource<GameObjectAssetHandle>> _pendingGameObjects = new();

        private UniTaskCompletionSource _unloadUnusedResources = new();
        
        public ResourcesManager(Container container, CancellationToken scope)
        {
            _container = container;
            _scope = scope;
        }
        
        public async UniTask DisposeAsync()
        {
            List<UniTask> tasks = new List<UniTask>();
            foreach (SceneAssetHandle handle in _scenes.Values)
            {
                tasks.Add(SceneManager.UnloadSceneAsync(handle.Scene)
                    .ToUniTask(cancellationToken: _scope));
            }
            _scenes.Clear();
            await UniTask.WhenAll(tasks);

            foreach (AudioClipAssetHandle handle in _audioClips.Values)
            {
                Resources.UnloadAsset(handle.AudioClip);
            }
            _audioClips.Clear();
            
            foreach (MeshAssetHandle handle in _meshes.Values)
            {
                Resources.UnloadAsset(handle.Mesh);
            }
            _meshes.Clear();
            
            foreach (SpriteAssetHandle handle in _sprites.Values)
            {
                Resources.UnloadAsset(handle.Sprite);
            }
            _sprites.Clear();
            
            foreach (SpriteAtlasAssetHandle? handle in _spriteAtlases.Values)
            {
                Resources.UnloadAsset(handle.SpriteAtlas);
            }
            _spriteAtlases.Clear();
            
            foreach (Texture2DAssetHandle handle in _textures.Values)
            {
                Resources.UnloadAsset(handle.Texture);
            }
            _textures.Clear();
            
            foreach (ScriptableObjectAssetHandle handle in _scriptableObjects.Values)
            {
                Resources.UnloadAsset(handle.ScriptableObject);
            }
            _scriptableObjects.Clear();
            
            foreach (GameObjectManager? manager in _gameObjectManagers.Values)
            {
                manager.Dispose();
            }
            _gameObjectManagers.Clear();
            _typedGameObjectManagers.Clear();
            _gameObject.Clear();

            _pendingAudioClips.Clear();
            _pendingMeshes.Clear();
            _pendingSprites.Clear();
            _pendingSpriteAtlases.Clear();
            _pendingTextures.Clear();
            _pendingScriptableObjects.Clear();
            _pendingGameObjects.Clear();
            
            await Resources.UnloadUnusedAssets()
                .ToUniTask();
        }
        
        public async UniTask<Scene> LoadScene(string resourcePath, CancellationToken token)
        {
            Scene scene = SceneManager.GetSceneByPath(resourcePath);

            if (!scene.isLoaded)
            {
                await SceneManager.LoadSceneAsync(scene.buildIndex, LoadSceneMode.Additive)
                    .ToUniTask(cancellationToken: token);
                
                scene = SceneManager.GetSceneByName(resourcePath);
            }

            if (!scene.isLoaded)
            {
                throw GameException.FromFormat("Failed to load scene '{0}' from resources.", resourcePath);
            }
            
            SceneAssetHandle handle = new SceneAssetHandle(this, resourcePath, scene);
            handle.Push(token);
            _scenes.Add(resourcePath, handle);
            return scene;
        }

        public async UniTask<AudioClip> LoadAudioClip(string resourcePath, CancellationToken token)
        {
            if (_audioClips.TryGetValue(resourcePath, out AudioClipAssetHandle? handle))
            {
                handle.Push(token);
                return handle.AudioClip;
            }

            if (_pendingAudioClips.TryGetValue(resourcePath, out var pending))
            {
                handle = await pending.Task.AttachExternalCancellation(token);
                handle.Push(token);
                return handle.AudioClip;
            }

            var tcs = new UniTaskCompletionSource<AudioClipAssetHandle>();
            _pendingAudioClips[resourcePath] = tcs;

            try
            {
                Object? result = await Resources.LoadAsync<AudioClip>(resourcePath)
                    .ToUniTask(cancellationToken: token);
                if (result == null)
                {
                    throw GameException.FromFormat("Failed to load AudioClip '{0}' from resources.", resourcePath);
                }
                Log.Assert(result is AudioClip, "result is AudioClip");

                AudioClip audioClip = (result as AudioClip)!;
                handle = new AudioClipAssetHandle(this, resourcePath, audioClip);
                handle.Push(token);
                _audioClips[resourcePath] = handle;
                tcs.TrySetResult(handle);
                return audioClip;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pendingAudioClips.Remove(resourcePath);
            }
        }

        public async UniTask<Mesh> LoadMesh(string resourcePath, CancellationToken token)
        {
            if (_meshes.TryGetValue(resourcePath, out MeshAssetHandle? handle))
            {
                handle.Push(token);
                return handle.Mesh;
            }

            if (_pendingMeshes.TryGetValue(resourcePath, out var pending))
            {
                handle = await pending.Task.AttachExternalCancellation(token);
                handle.Push(token);
                return handle.Mesh;
            }

            var tcs = new UniTaskCompletionSource<MeshAssetHandle>();
            _pendingMeshes[resourcePath] = tcs;

            try
            {
                Object? result = await Resources.LoadAsync<Mesh>(resourcePath)
                    .ToUniTask(cancellationToken: token);
                if (result == null)
                {
                    throw GameException.FromFormat("Failed to load Mesh '{0}' from resources.", resourcePath);
                }
                Log.Assert(result is Mesh, "result is Mesh");

                Mesh mesh = (result as Mesh)!;
                handle = new MeshAssetHandle(this, resourcePath, mesh);
                handle.Push(token);
                _meshes[resourcePath] = handle;
                tcs.TrySetResult(handle);
                return mesh;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pendingMeshes.Remove(resourcePath);
            }
        }
        
        public async UniTask<Sprite> LoadSprite(string resourcePath, CancellationToken token)
        {
            if (_sprites.TryGetValue(resourcePath, out SpriteAssetHandle? handle))
            {
                handle.Push(token);
                return handle.Sprite;
            }

            if (_pendingSprites.TryGetValue(resourcePath, out var pending))
            {
                handle = await pending.Task.AttachExternalCancellation(token);
                handle.Push(token);
                return handle.Sprite;
            }

            var tcs = new UniTaskCompletionSource<SpriteAssetHandle>();
            _pendingSprites[resourcePath] = tcs;

            try
            {
                Object? result = await Resources.LoadAsync<Sprite>(resourcePath)
                    .ToUniTask(cancellationToken: token);
                if (result == null)
                {
                    throw GameException.FromFormat("Failed to load Sprite '{0}' from resources.", resourcePath);
                }
                Log.Assert(result is Sprite, "result is Sprite");

                Sprite sprite = (result as Sprite)!;
                handle = new SpriteAssetHandle(this, resourcePath, sprite);
                handle.Push(token);
                _sprites[resourcePath] = handle;
                tcs.TrySetResult(handle);
                return sprite;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pendingSprites.Remove(resourcePath);
            }
        }
        
        public async UniTask<Sprite> LoadAtlasedSprite(string resourcePath, string spriteName, CancellationToken token)
        {
            SpriteAtlas atlas = await LoadSpriteAtlas(resourcePath, token);
            Sprite? sprite = atlas.GetSprite(spriteName);
            if (sprite == null)
            {
                throw GameException.FromFormat("Failed to locate Sprite '{0}' in SpriteAtlas '{1}' from resources.", spriteName, resourcePath);
            }
            
            return sprite;
        }
        
        public async UniTask<SpriteAtlas> LoadSpriteAtlas(string resourcePath, CancellationToken token)
        {
            if (_spriteAtlases.TryGetValue(resourcePath, out SpriteAtlasAssetHandle? handle))
            {
                handle.Push(token);
                return handle.SpriteAtlas;
            }

            if (_pendingSpriteAtlases.TryGetValue(resourcePath, out var pending))
            {
                handle = await pending.Task.AttachExternalCancellation(token);
                handle.Push(token);
                return handle.SpriteAtlas;
            }

            var tcs = new UniTaskCompletionSource<SpriteAtlasAssetHandle>();
            _pendingSpriteAtlases[resourcePath] = tcs;

            try
            {
                Object? result = await Resources.LoadAsync<SpriteAtlas>(resourcePath)
                    .ToUniTask(cancellationToken: token);
                if (result == null)
                {
                    throw GameException.FromFormat("Failed to load SpriteAtlas '{0}' from resources.", resourcePath);
                }
                Log.Assert(result is SpriteAtlas, "result is Sprite");

                SpriteAtlas spriteAtlas = (result as SpriteAtlas)!;
                handle = new SpriteAtlasAssetHandle(this, resourcePath, spriteAtlas);
                handle.Push(token);
                _spriteAtlases[resourcePath] = handle;
                tcs.TrySetResult(handle);
                return spriteAtlas;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pendingSpriteAtlases.Remove(resourcePath);
            }
        }
        
        public async UniTask<Texture2D> LoadTexture2D(string resourcePath, CancellationToken token)
        {
            if (_textures.TryGetValue(resourcePath, out Texture2DAssetHandle? handle))
            {
                handle.Push(token);
                return handle.Texture;
            }

            if (_pendingTextures.TryGetValue(resourcePath, out var pending))
            {
                handle = await pending.Task.AttachExternalCancellation(token);
                handle.Push(token);
                return handle.Texture;
            }

            var tcs = new UniTaskCompletionSource<Texture2DAssetHandle>();
            _pendingTextures[resourcePath] = tcs;

            try
            {
                Object? result = await Resources.LoadAsync<Texture2D>(resourcePath)
                    .ToUniTask(cancellationToken: token);
                if (result == null)
                {
                    throw GameException.FromFormat("Failed to load Texture2D '{0}' from resources.", resourcePath);
                }
                Log.Assert(result is Texture2D, "result is Texture2D");

                Texture2D texture2D = (result as Texture2D)!;
                handle = new Texture2DAssetHandle(this, resourcePath, texture2D);
                handle.Push(token);
                _textures[resourcePath] = handle;
                tcs.TrySetResult(handle);
                return texture2D;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pendingTextures.Remove(resourcePath);
            }
        }

        public async UniTask<ScriptableObject> LoadScriptableObject(string resourcePath, CancellationToken token)
        {
            if (_scriptableObjects.TryGetValue(resourcePath, out ScriptableObjectAssetHandle? handle))
            {
                handle.Push(token);
                return handle.ScriptableObject;
            }

            if (_pendingScriptableObjects.TryGetValue(resourcePath, out var pending))
            {
                handle = await pending.Task.AttachExternalCancellation(token);
                handle.Push(token);
                return handle.ScriptableObject;
            }

            var tcs = new UniTaskCompletionSource<ScriptableObjectAssetHandle>();
            _pendingScriptableObjects[resourcePath] = tcs;

            try
            {
                Object? result = await Resources.LoadAsync<ScriptableObject>(resourcePath)
                    .ToUniTask(cancellationToken: token);
                if (result == null)
                {
                    throw GameException.FromFormat("Failed to load ScriptableObject '{0}' from resources.", resourcePath);
                }
                Log.Assert(result is ScriptableObject, "result is ScriptableObject");

                ScriptableObject scriptableObject = (result as ScriptableObject)!;
                handle = new ScriptableObjectAssetHandle(this, resourcePath, scriptableObject);
                handle.Push(token);
                _scriptableObjects[resourcePath] = handle;
                tcs.TrySetResult(handle);
                return scriptableObject;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pendingScriptableObjects.Remove(resourcePath);
            }
        }
        
        public async UniTask<GameObject> LoadGameObject(string resourcePath, CancellationToken token)
        {
            if (_gameObject.TryGetValue(resourcePath, out GameObjectAssetHandle? handle))
            {
                handle.Push(token);
                return handle.Prefab;
            }

            if (_pendingGameObjects.TryGetValue(resourcePath, out var pending))
            {
                handle = await pending.Task.AttachExternalCancellation(token);
                handle.Push(token);
                return handle.Prefab;
            }

            var tcs = new UniTaskCompletionSource<GameObjectAssetHandle>();
            _pendingGameObjects[resourcePath] = tcs;

            try
            {
                Object? result = await Resources.LoadAsync<GameObject>(resourcePath)
                    .ToUniTask(cancellationToken: token);
                if (result == null)
                {
                    throw GameException.FromFormat("Failed to load GameObject '{0}' from resources.", resourcePath);
                }
                Log.Assert(result is GameObject, "result is GameObject");

                GameObject prefab = (result as GameObject)!;
                handle = new GameObjectAssetHandle(this, resourcePath, prefab);
                handle.Push(token);
                _gameObject[resourcePath] = handle;
                tcs.TrySetResult(handle);
                return prefab;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pendingGameObjects.Remove(resourcePath);
            }
        }
        
        public async UniTask<GameObjectManager> LoadGameObjectManager(GameObjectAssetRef assetRef,
            GameObjectManagerOptions options, CancellationToken token)
        {
            if (!_gameObjectManagers.TryGetValue(assetRef._resourcesPath, out GameObjectManager? manager))
            {
                manager = new GameObjectManager(_container, assetRef, options);
                _gameObjectManagers.Add(assetRef._resourcesPath, manager);
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
            if (!_typedGameObjectManagers.TryGetValue(assetRef._resourcesPath, out Dictionary<Type, UniTaskCompletionSource<object>>? managers))
            {
                managers = new Dictionary<Type, UniTaskCompletionSource<object>>();
                _typedGameObjectManagers.Add(assetRef._resourcesPath, managers);
            }

            GameObjectManagerT<T> manager;
            if (!managers.TryGetValue(typeof(T), out UniTaskCompletionSource<object>? taskCompletionSource))
            {
                taskCompletionSource = new UniTaskCompletionSource<object>();
                managers.Add(typeof(T), taskCompletionSource);
                

                try
                {
                    GameObjectManager proxy = await LoadGameObjectManager((GameObjectAssetRef)assetRef, options, token);
                    manager = new GameObjectManagerT<T>(proxy);
                    taskCompletionSource.TrySetResult(manager);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    taskCompletionSource.TrySetException(e);
                    throw;
                }
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

        private void ReleaseScene(SceneAssetHandle handle)
        {
            DelayedReleaseScene(handle).Forget();
        }

        private void ReleaseAudioClip(AudioClipAssetHandle handle)
        {
            DelayedReleaseAudioClip(handle).Forget();
        }

        private void ReleaseMesh(MeshAssetHandle handle)
        {
            DelayedReleaseMesh(handle).Forget();
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

        private async UniTaskVoid DelayedReleaseScene(SceneAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_scenes.Remove(handle.ResourcesPath))
            {
                return;
            }

            await SceneManager.UnloadSceneAsync(handle.Scene)
                .ToUniTask(cancellationToken: _scope);

            _unloadUnusedResources.TrySetResult();
        }
        private async UniTaskVoid DelayedReleaseAudioClip(AudioClipAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_audioClips.Remove(handle.ResourcesPath))
            {
                return;
            }

            Resources.UnloadAsset(handle.AudioClip);
            _unloadUnusedResources.TrySetResult();
        }
        
        private async UniTaskVoid DelayedReleaseMesh(MeshAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_meshes.Remove(handle.ResourcesPath))
            {
                return;
            }

            Resources.UnloadAsset(handle.Mesh);
            _unloadUnusedResources.TrySetResult();
        }
        
        private async UniTaskVoid DelayedReleaseSprite(SpriteAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_sprites.Remove(handle.ResourcesPath))
            {
                return;
            }

            Resources.UnloadAsset(handle.Sprite);
            _unloadUnusedResources.TrySetResult();
        }
        
        private async UniTaskVoid DelayedReleaseSpriteAtlas(SpriteAtlasAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_textures.Remove(handle.ResourcesPath))
            {
                return;
            }

            Resources.UnloadAsset(handle.SpriteAtlas);
            _unloadUnusedResources.TrySetResult();
        }
        
        private async UniTaskVoid DelayedReleaseTexture2D(Texture2DAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_textures.Remove(handle.ResourcesPath))
            {
                return;
            }

            Resources.UnloadAsset(handle.Texture);
            _unloadUnusedResources.TrySetResult();
        }
        
        private async UniTaskVoid DelayedReleaseScriptableObject(ScriptableObjectAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_scriptableObjects.Remove(handle.ResourcesPath))
            {
                return;
            }

            Resources.UnloadAsset(handle.ScriptableObject);
            _unloadUnusedResources.TrySetResult();
        }
        
        private async UniTaskVoid DelayedReleaseGameObject(GameObjectAssetHandle handle)
        {
            Log.Assert(handle.Count == 0, "handle.Count ({0}) == 0", handle.Count);

            if (!_scope.IsCancellationRequested)
            {
                await UniTask.DelayFrame(2, PlayerLoopTiming.EarlyUpdate, _scope);
                
                if (handle.Count != 0)
                {
                    return;
                }
            }
            
            if (!_gameObject.Remove(handle.ResourcesPath))
            {
                return;
            }
            
            _unloadUnusedResources.TrySetResult();
        }

        public async UniTask UnloadUnusedAssets(CancellationToken token)
        {
            await _unloadUnusedResources.Task
                .AttachExternalCancellation(token);
            _unloadUnusedResources = new();

            await Resources.UnloadUnusedAssets()
                .ToUniTask(cancellationToken: token);
        }
    }
}
#nullable disable