#nullable enable
using System.Threading;
using DeepForestLabs.Logger;
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
        private abstract class AssetHandle<T, U>
            where T : AssetReference
        {
            private int _count;
            public string Guid => LoadHandle.Guid;
            private LoadHandle<T, U> LoadHandle { get; }
            protected AddressablesManager AddressablesManager { get; }
            public T AssetReference => LoadHandle.DownloadHandle.AssetReference;
            public long Size => LoadHandle.DownloadHandle.Size;
            public IResourceLocation Location => LoadHandle.Location;
            public AsyncOperationHandle<U> OperationHandle { get; }
            public U Asset { get; }
            public int Count => _count;

            protected AssetHandle(AddressablesManager addressablesManager, LoadHandle<T, U> loadHandle, 
                AsyncOperationHandle<U> operationHandle)
            {
                Log.Assert(operationHandle.IsValid(), loadHandle.GetContext(), "operationHandle.IsValid()");
                Log.Assert(operationHandle.Status == AsyncOperationStatus.Succeeded, loadHandle.GetContext(), "operationHandle.Status == AsyncOperationStatus.Succeeded");

                AddressablesManager = addressablesManager;
                LoadHandle = loadHandle;
                OperationHandle = operationHandle;
                Asset = OperationHandle.Result;
                _count = 0;
            }
    
            public void Push(CancellationToken token)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Interlocked.Increment(ref _count);
                token.Register(static s => ((AssetHandle<T, U>)s!).Pop(), this);
            }
            
            private void Pop()
            {
                Interlocked.Decrement(ref _count);
                if (_count == 0)
                {
                    Release();
                }
                
                _count = Mathf.Max(0, _count);
            }

            protected abstract void Release();
            
            public Object? GetContext() => LoadHandle.GetContext();
        }
        
        private sealed class SceneAssetHandle : AssetHandle<AssetReference, SceneInstance>
        {
            public SceneAssetHandle(AddressablesManager addressablesManager, 
                SceneLoadHandle loadHandle, AsyncOperationHandle<SceneInstance> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }

            protected override void Release()
            {
                AddressablesManager.ReleaseScene(this);
            }
        }
        
        private sealed class AudioClipAssetHandle : AssetHandle<AssetReferenceT<AudioClip>, AudioClip>
        {
            public AudioClipAssetHandle(AddressablesManager addressablesManager, 
                AudioClipLoadHandle loadHandle, AsyncOperationHandle<AudioClip> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }
            
            protected override void Release()
            {
                AddressablesManager.ReleaseAudioClip(this);
            }
        }
        
        private sealed class MeshAssetHandle : AssetHandle<AssetReferenceT<Mesh>, Mesh>
        {
            public MeshAssetHandle(AddressablesManager addressablesManager, 
                MeshLoadHandle loadHandle, AsyncOperationHandle<Mesh> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }
            
            protected override void Release()
            {
                AddressablesManager.ReleaseMesh(this);
            }
        }
        
        private sealed class SpriteAssetHandle : AssetHandle<AssetReferenceSprite, Sprite>
        {
            public SpriteAssetHandle(AddressablesManager addressablesManager, 
                SpriteLoadHandle loadHandle, AsyncOperationHandle<Sprite> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }
            
            protected override void Release()
            {
                AddressablesManager.ReleaseSprite(this);
            }
        }
        
        private sealed class Texture2DAssetHandle : AssetHandle<AssetReferenceTexture2D, Texture2D>
        {
            public Texture2DAssetHandle(AddressablesManager addressablesManager, 
                Texture2DLoadHandle loadHandle, AsyncOperationHandle<Texture2D> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }
            
            protected override void Release()
            {
                AddressablesManager.ReleaseTexture2D(this);
            }
        }
        
        private sealed class SpriteAtlasAssetHandle : AssetHandle<AssetReferenceT<SpriteAtlas>, SpriteAtlas>
        {
            public SpriteAtlasAssetHandle(AddressablesManager addressablesManager, 
                SpriteAtlasLoadHandle loadHandle, AsyncOperationHandle<SpriteAtlas> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }
            
            protected override void Release()
            {
                AddressablesManager.ReleaseSpriteAtlas(this);
            }
        }

        private sealed class ScriptableObjectAssetHandle : AssetHandle<AssetReferenceT<ScriptableObject>, ScriptableObject>
        {
            public ScriptableObjectAssetHandle(AddressablesManager addressablesManager, 
                ScriptableObjectLoadHandle loadHandle, AsyncOperationHandle<ScriptableObject> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }
            
            protected override void Release()
            {
                AddressablesManager.ReleaseScriptableObject(this);
            }
        }

        private sealed class GameObjectAssetHandle : AssetHandle<AssetReferenceGameObject, GameObject>
        {
            public GameObjectAssetHandle(AddressablesManager addressablesManager, 
                GameObjectLoadHandle loadHandle, AsyncOperationHandle<GameObject> operationHandle) 
                : base(addressablesManager, loadHandle, operationHandle)
            {
            }
            
            protected override void Release()
            {
                AddressablesManager.ReleaseGameObject(this);
            }
        }
    }
}
#nullable disable