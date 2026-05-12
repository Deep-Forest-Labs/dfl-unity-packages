#nullable enable
using System.Threading;
using ZLinq;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder
    {
        public IContainerBuilder AddDownload(SceneAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }
        
        public IContainerBuilder AddDownload(AudioClipAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }
        
        public IContainerBuilder AddDownload(MeshAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }
        
        public IContainerBuilder AddDownload(SpriteAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }
        
        public IContainerBuilder AddDownload(AtlasedSpriteAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }
        
        public IContainerBuilder AddDownload(SpriteAtlasAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }
        
        public IContainerBuilder AddDownload(Texture2DAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }

        public IContainerBuilder AddDownload<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ScriptableObject
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }

        public IContainerBuilder AddDownload(GameObjectAssetRef assetRef)
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }

        public IContainerBuilder AddDownload<T>(GameObjectAssetRefT<T> assetRef) where T : Component
        {
            if (assetRef._mode == AssetRefMode.Resources)
            {
                return this;
            }
            
            Log.Assert(_container._addressablesManager != null, "_container._addressablesManager != null");
            
            _downloads ??= new();
            _downloads.Add(_container.Download(assetRef, _container.Scope));
            return this;
        }

        public IContainerBuilder AddScene(SceneAssetRef assetRef)
        {
            UniTask<Scene> SceneLoadFactory(IDiCollection c, CancellationToken t)
            {
                return _container.Checkout(assetRef, t); 
            }

            return AddDownload(assetRef)
                .AddScoped(SceneLoadFactory);
        }
        
        public IContainerBuilder AddScene<T>(SceneAssetRef assetRef)
            where T : class
        {
            UniTask<Scene> SceneLoadFactory(IDiCollection c, CancellationToken t)
            {
                return _container.Checkout(assetRef, t); 
            }

            return AddDownload(assetRef)
                .AddScoped(SceneLoadFactory)
                .AddScoped(Find<T>);
        }
        
        public IContainerBuilder AddScene<T1, T2>(SceneAssetRef assetRef)
            where T1 : class
            where T2 : class
        {
            UniTask<Scene> SceneLoadFactory(IDiCollection c, CancellationToken t)
            {
                return _container.Checkout(assetRef, t); 
            }

            return AddDownload(assetRef)
                .AddScoped(SceneLoadFactory)
                .AddScoped(Find<T1>)
                .AddScoped(Find<T2>);
        }
        
        public IContainerBuilder AddAudioClip(AudioClipAssetRef assetRef)
        {
            UniTask<AudioClip> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, t);
            }

            return AddDownload(assetRef) 
                .AddScoped(Resolver);
        }

        public IContainerBuilder AddMesh(MeshAssetRef assetRef)
        {
            UniTask<Mesh> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, t);
            }

            return AddDownload(assetRef) 
                .AddScoped(Resolver);
        }

        public IContainerBuilder AddSprite(SpriteAssetRef assetRef)
        {
            UniTask<Sprite> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, t);
            }
            
            return AddDownload(assetRef) 
                .AddScoped(Resolver);
        }
        
        public IContainerBuilder AddAtlasedSprite(AtlasedSpriteAssetRef assetRef)
        {
            UniTask<Sprite> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, t);
            }

            return AddDownload(assetRef) 
                .AddScoped(Resolver);
        }

        public IContainerBuilder AddSpriteAtlas(SpriteAtlasAssetRef assetRef)
        {
            UniTask<SpriteAtlas> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, t);
            }
            
            return AddDownload(assetRef) 
                .AddScoped(Resolver);
        }

        public IContainerBuilder AddTextures2D(Texture2DAssetRef assetRef)
        {
            UniTask<Texture2D> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, t);
            }
            
            return AddDownload(assetRef) 
                .AddScoped(Resolver);
        }
        
        public IContainerBuilder AddScriptableObject<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ScriptableObject
        {
            Log.Assert(!typeof(MonoBehaviour).IsAssignableFrom(typeof(T)), "Please use AddGameObjectAssetHandle<{0}>() instead.");
        
            UniTask<T> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, t);
            }
            
            return AddDownload(assetRef) 
                .AddScoped(Resolver);
        }

        public IContainerBuilder AddGameObjectManager(GameObjectAssetRef assetRef, GameObjectManagerOptions options)
        {
            UniTask<IGameObjectManager> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, options, t);
            }

            return AddSingleton(Resolver);
        }

        public IContainerBuilder AddGameObjectManager<T>(GameObjectAssetRefT<T> assetRef, GameObjectManagerOptions options) where T : Component
        {
            UniTask<IGameObjectManagerT<T>> Resolver(IDiCollection _, CancellationToken t)
            {
                return _container.Checkout(assetRef, options, t);
            }

            return AddSingleton(Resolver);
        }

        public IContainerBuilder AddScopedGameObject(GameObjectAssetRef assetRef, Transform? parent,
            bool worldPositionStays = true)
        {
            GameObjectManagerOptions options = GameObjectManagerOptions.Required;
            
            if (assetRef._mode == AssetRefMode.Addressables && options.DownloadOptions == GameObjectManagerDownloadOptions.Required)
            {
                AddDownload(assetRef);
            }
            
            async UniTask<GameObject> Resolver(IDiCollection _, CancellationToken t)
            {
                IGameObjectManager result = await _container.Checkout(assetRef, options, t);
                                
                return await result.Checkout(parent, worldPositionStays, t);
            }

            return AddScoped(Resolver);
        }
        
        public IContainerBuilder AddScopedGameObject<T>(GameObjectAssetRefT<T> assetRef, Transform? parent,
            bool worldPositionStays = true) where T : Component
        {
            GameObjectManagerOptions options = GameObjectManagerOptions.Required;
            
            async UniTask<T> Resolver(IDiCollection _, CancellationToken t)
            {
                IGameObjectManagerT<T> result = await _container.Checkout(assetRef, options, t);
                
                return await result.Checkout(parent, worldPositionStays, t);
            }
            
            return AddScoped(Resolver);
        }
        
        private static async UniTask<T> Find<T>(IDiCollection collection, CancellationToken token) where T : class
        {
            Scene scene;
            while (!collection.TryGet(out scene) && !scene.IsValid() || !scene.isLoaded)
            {
                await UniTask.NextFrame();
            }

            GameObject[] gameObjects = scene.GetRootGameObjects();

            foreach (GameObject gameObject in gameObjects)
            {
                T? behaviour = gameObject.GetComponents<MonoBehaviour>().AsValueEnumerable().OfType<T>().FirstOrDefault();
                if (behaviour != null)
                {
                    return behaviour;
                }
            }

            throw GameException.FromFormat("Failed to find script of type {0} in scene {1}.", typeof(T).Name, scene.name);
        }
    }
}
#nullable disable