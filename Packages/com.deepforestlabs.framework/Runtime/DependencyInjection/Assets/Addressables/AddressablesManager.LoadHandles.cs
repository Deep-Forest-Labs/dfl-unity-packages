#nullable enable
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager
    {
        private abstract class LoadHandle<T, U>
            where T : AssetReference
        {
            public string Guid => DownloadHandle.Guid;
            public IResourceLocation Location { get; }
            public DownloadHandle<T> DownloadHandle { get; }
            public T AssetReference => DownloadHandle.AssetReference;

            protected readonly AddressablesManager _manager;
            private int _referenceCount = 0;
            private CancellationTokenSource? _cancellationTokenSource;
            private UniTaskCompletionSource<U>? _taskCompletionSource;
            public int ReferenceCount => _referenceCount;

            protected LoadHandle(AddressablesManager manager, DownloadHandle<T> downloadHandle, IResourceLocation location)
            {
                _manager = manager;
                DownloadHandle = downloadHandle;
                Location = location;
            }

            public UniTask WaitUntilLoaded(CancellationToken token)
            {
                Log.Assert(_cancellationTokenSource != null || _referenceCount == 0, "_referenceCount == 0");
                Interlocked.Increment(ref _referenceCount);
                if (_cancellationTokenSource == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _taskCompletionSource = new UniTaskCompletionSource<U>();
                    LoadInBackground(_cancellationTokenSource.Token);
                }
                Log.Assert(_taskCompletionSource != null, "_taskCompletionSource != null");
                
                token.Register(static s => ((LoadHandle<T, U>)s!).Pop(), this);

                return _taskCompletionSource.Task.AttachExternalCancellation(token);
            }

            public void OnLoadComplete(AsyncOperationHandle<U> operation)
            {
                Log.Assert(operation.IsValid(), "operation.IsValid()");
                Log.Assert(operation.Status == AsyncOperationStatus.Succeeded, "operation.Status == AsyncOperationStatus.Succeeded");
                _taskCompletionSource?.TrySetResult(operation.Result);
            }
            
            public Object? GetContext() => DownloadHandle.EditorContext;

            protected abstract void LoadInBackground(CancellationToken token);
            
            private void Pop()
            {
                Interlocked.Decrement(ref _referenceCount);
                if (_referenceCount == 0)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                    _taskCompletionSource = null;
                }
                
                _referenceCount = Mathf.Max(0, _referenceCount);
            }
        }

        private sealed class SceneLoadHandle : LoadHandle<AssetReference, SceneInstance>
        {
            public SceneLoadHandle(AddressablesManager manager, SceneDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }

            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadSceneInBackground(this, token).Forget();
            }
        }

        private sealed class AudioClipLoadHandle : LoadHandle<AssetReferenceT<AudioClip>, AudioClip>
        {
            public AudioClipLoadHandle(AddressablesManager manager, AudioClipDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadAudioClipInBackground(this, token).Forget();
            }
        }

        private sealed class MeshLoadHandle : LoadHandle<AssetReferenceT<Mesh>, Mesh>
        {
            public MeshLoadHandle(AddressablesManager manager, MeshDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadMeshInBackground(this, token).Forget();
            }
        }

        private sealed class RuntimeAnimatorControllerLoadHandle : LoadHandle<AssetReferenceT<RuntimeAnimatorController>, RuntimeAnimatorController>
        {
            public RuntimeAnimatorControllerLoadHandle(AddressablesManager manager, RuntimeAnimatorControllerDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadRuntimeAnimatorControllerInBackground(this, token).Forget();
            }
        }

        private sealed class SpriteLoadHandle : LoadHandle<AssetReferenceSprite, Sprite>
        {
            public SpriteLoadHandle(AddressablesManager manager, SpriteDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadSpriteInBackground(this, token).Forget();
            }
        }

        private sealed class SpriteAtlasLoadHandle : LoadHandle<AssetReferenceT<SpriteAtlas>, SpriteAtlas>
        {
            public SpriteAtlasLoadHandle(AddressablesManager manager, SpriteAtlasDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadSpriteAtlasInBackground(this, token).Forget();
            }
        }
        
        private sealed class Texture2DLoadHandle : LoadHandle<AssetReferenceTexture2D, Texture2D>
        {
            public Texture2DLoadHandle(AddressablesManager manager, Texture2DDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadTexture2DInBackground(this, token).Forget();
            }
        }

        private sealed class ScriptableObjectLoadHandle : LoadHandle<AssetReferenceT<ScriptableObject>, ScriptableObject>
        {
            public ScriptableObjectLoadHandle(AddressablesManager manager, ScriptableObjectDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadScriptableObjectInBackground(this, token).Forget();
            }
        }

        private sealed class GameObjectLoadHandle : LoadHandle<AssetReferenceGameObject, GameObject>
        {
            public GameObjectLoadHandle(AddressablesManager manager, GameObjectDownloadHandle downloadHandle, IResourceLocation location) 
                : base(manager, downloadHandle, location)
            {
            }
            
            protected override void LoadInBackground(CancellationToken token)
            {
                _manager.LoadGameObjectInBackground(this, token).Forget();
            }
        }
    }
}