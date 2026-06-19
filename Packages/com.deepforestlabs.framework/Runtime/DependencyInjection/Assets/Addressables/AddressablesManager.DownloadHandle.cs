#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed partial class AddressablesManager
    {
        private interface IDownloadHandle
        {
            string Guid { get; }
            AssetReference AssetReference { get; }
            Type AssetType { get; }
            //long Size { get; set; }
            Object? EditorContext { get; }

            void OnDownloadComplete(ResultV<IReadOnlyList<IResourceLocation>> locationsResult);
        }
        
        private abstract class DownloadHandle<T> : IDownloadHandle
            where T : AssetReference
        {
            public string Guid { get; }
            public T AssetReference { get; }
            AssetReference IDownloadHandle.AssetReference => AssetReference;
            public Type AssetType { get; }
            public long Size { get; set; }
            public Object? EditorContext { get; private set; }

            private readonly UniTaskCompletionSource<IReadOnlyList<IResourceLocation>> _taskCompletionSource = new();

            protected DownloadHandle(string guid, T assetReference, Type assetType, Object? editorContext)
            {
                Guid = guid;
                AssetReference = assetReference;
                AssetType = assetType;
                EditorContext = editorContext;
            }

            public UniTask<IReadOnlyList<IResourceLocation>> WaitUntilCached(CancellationToken token)
            {
                return _taskCompletionSource.Task.AttachExternalCancellation(token);
            }

            public void OnDownloadComplete(ResultV<IReadOnlyList<IResourceLocation>> locationsResult)
            {
                if (locationsResult.IsValid)
                {
                    _taskCompletionSource.TrySetResult(locationsResult.Value);
                }
                else
                {
                    _taskCompletionSource.TrySetException(new GameException(locationsResult.Error));
                }
            }

            public override string ToString()
            {
                #if UNITY_EDITOR
                return UnityEditor.AssetDatabase.GetAssetPath(EditorContext);
                #else
                return Guid;
                #endif
            }
        }

        private sealed class SceneDownloadHandle : DownloadHandle<AssetReference>
        {
            public SceneDownloadHandle(string guid, AssetReference assetReference) 
                : base(guid, assetReference, typeof(SceneInstance), _sceneAssetRefContextProvider?.Invoke(guid))
            {
            }
        }

        private sealed class AudioClipDownloadHandle : DownloadHandle<AssetReferenceT<AudioClip>>
        {
            public AudioClipDownloadHandle(string guid, AssetReferenceT<AudioClip> assetReference) 
                : base(guid, assetReference, typeof(AudioClip), _audioClipAssetRefContextProvider?.Invoke(guid))
            {
            }
        }

        private sealed class MeshDownloadHandle : DownloadHandle<AssetReferenceT<Mesh>>
        {
            public MeshDownloadHandle(string guid, AssetReferenceT<Mesh> assetReference) 
                : base(guid, assetReference, typeof(Mesh), _meshAssetRefContextProvider?.Invoke(guid))
            {
            }
        }
        
        private sealed class RuntimeAnimatorControllerDownloadHandle : DownloadHandle<AssetReferenceT<RuntimeAnimatorController>>
        {
            public RuntimeAnimatorControllerDownloadHandle(string guid, AssetReferenceT<RuntimeAnimatorController> assetReference) 
                : base(guid, assetReference, typeof(RuntimeAnimatorController), _runtimeAnimatorControllerAssetRefContextProvider?.Invoke(guid))
            {
            }
        }
        
        private sealed class SpriteDownloadHandle : DownloadHandle<AssetReferenceSprite>
        {
            public SpriteDownloadHandle(string guid, AssetReferenceSprite assetReference) 
                : base(guid, assetReference, typeof(Sprite), _spriteAssetRefContextProvider?.Invoke(guid))
            {
            }
        }
        
        private sealed class SpriteAtlasDownloadHandle : DownloadHandle<AssetReferenceT<SpriteAtlas>>
        {
            public SpriteAtlasDownloadHandle(string guid, AssetReferenceT<SpriteAtlas> assetReference) 
                : base(guid, assetReference, typeof(SpriteAtlas), _spriteAtlasAssetRefContextProvider?.Invoke(guid))
            {
            }
        }
        
        private sealed class Texture2DDownloadHandle : DownloadHandle<AssetReferenceTexture2D>
        {
            public Texture2DDownloadHandle(string guid, AssetReferenceTexture2D assetReference) 
                : base(guid, assetReference, typeof(Texture2D), _texture2DAssetRefContextProvider?.Invoke(guid))
            {
            }
        }
        
        private sealed class ScriptableObjectDownloadHandle : DownloadHandle<AssetReferenceT<ScriptableObject>>
        {
            public ScriptableObjectDownloadHandle(string guid, AssetReferenceT<ScriptableObject> assetReference) 
                : base(guid, assetReference, typeof(ScriptableObject), _scriptableObjectAssetRefContextProvider?.Invoke(guid))
            {
            }
        }
        
        private sealed class GameObjectDownloadHandle : DownloadHandle<AssetReferenceGameObject>
        {
            public GameObjectDownloadHandle(string guid, AssetReferenceGameObject assetReference) 
                : base(guid, assetReference, typeof(GameObject), _gameObjectAssetRefContextProvider?.Invoke(guid))
            {
            }
        }
    }
}