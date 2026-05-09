#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace DeepForestLabs
{
    public partial interface IContainer
    {
        bool CanLocate(SceneAssetRef assetRef);
        
        bool CanLocate(AudioClipAssetRef assetRef);
        
        bool CanLocate(MeshAssetRef assetRef);
        
        bool CanLocate(SpriteAssetRef assetRef);
        
        bool CanLocate(AtlasedSpriteAssetRef assetRef);
        
        bool CanLocate(Texture2DAssetRef assetRef);
        
        bool CanLocate(SpriteAtlasAssetRef assetRef);

        bool CanLocate(ScriptableObjectAssetRef assetRef);
        
        bool CanLocate(GameObjectAssetRef assetRef);
        
        IEnumerable<IResourceLocation> GetLocations(SceneAssetRef assetRef);
        
        IEnumerable<IResourceLocation> GetLocations(AudioClipAssetRef assetRef);
        
        IEnumerable<IResourceLocation> GetLocations(MeshAssetRef assetRef);
        
        IEnumerable<IResourceLocation> GetLocations(SpriteAssetRef assetRef);
        
        IEnumerable<IResourceLocation> GetLocations(Texture2DAssetRef assetRef);
        
        IEnumerable<IResourceLocation> GetLocations(SpriteAtlasAssetRef assetRef);

        IEnumerable<IResourceLocation> GetLocations(ScriptableObjectAssetRef assetRef);
        
        IEnumerable<IResourceLocation> GetLocations(GameObjectAssetRef assetRef);
        
        UniTask Download(SceneAssetRef assetRef, CancellationToken token);
		
        UniTask Download(AudioClipAssetRef assetRef, CancellationToken token);
		
        UniTask Download(MeshAssetRef assetRef, CancellationToken token);
        
        UniTask Download(SpriteAssetRef assetRef, CancellationToken token);
        
        UniTask Download(AtlasedSpriteAssetRef assetRef, CancellationToken token);
        
        UniTask Download(SpriteAtlasAssetRef assetRef, CancellationToken token);
        
        UniTask Download(Texture2DAssetRef assetRef, CancellationToken token);
        
        UniTask Download(ScriptableObjectAssetRef assetRef, CancellationToken token);
        
        UniTask Download(GameObjectAssetRef assetRef, CancellationToken token);
        
        UniTask<Scene> Checkout(SceneAssetRef assetRef, CancellationToken token);
        
        UniTask<AudioClip> Checkout(AudioClipAssetRef assetRef, CancellationToken token);
        
        UniTask<Mesh> Checkout(MeshAssetRef assetRef, CancellationToken token);
        
        UniTask<Sprite> Checkout(SpriteAssetRef assetRef, CancellationToken token);
        
        UniTask<Sprite> Checkout(SpriteAssetRef assetRef, Image? image, CancellationToken token);
        
        UniTask<Sprite> Checkout(SpriteAssetRef assetRef, SpriteRenderer spriteRenderer, CancellationToken token);
        
        UniTask<Sprite> Checkout(AtlasedSpriteAssetRef assetRef, CancellationToken token);
        
        UniTask<Sprite> Checkout(AtlasedSpriteAssetRef assetRef, Image? image, CancellationToken token);
        
        UniTask<Sprite> Checkout(AtlasedSpriteAssetRef assetRef, SpriteRenderer spriteRenderer, CancellationToken token);
        
        UniTask<SpriteAtlas> Checkout(SpriteAtlasAssetRef assetRef, CancellationToken token);

        UniTask<Sprite> Checkout(SpriteAtlasAssetRef assetRef, string spriteName, CancellationToken token)
            => Checkout(AtlasedSpriteAssetRef.FromAtlas(assetRef, spriteName), token);
        
        UniTask<Sprite> Checkout(SpriteAtlasAssetRef assetRef, string spriteName, Image? image, CancellationToken token)
            => Checkout(AtlasedSpriteAssetRef.FromAtlas(assetRef, spriteName), image, token);
        
        UniTask<Sprite> Checkout(SpriteAtlasAssetRef assetRef, string spriteName, SpriteRenderer spriteRenderer, CancellationToken token)
            => Checkout(AtlasedSpriteAssetRef.FromAtlas(assetRef, spriteName), spriteRenderer, token);
        
        UniTask<Texture2D> Checkout(Texture2DAssetRef assetRef, CancellationToken token);
        
        UniTask<Texture2D> Checkout(Texture2DAssetRef assetRef, RawImage? image, CancellationToken token);

        UniTask<T> Checkout<T>(ScriptableObjectAssetRefT<T> assetRef, CancellationToken token) where T : ScriptableObject;
        
        UniTask<IGameObjectManager> Checkout(GameObjectAssetRef assetRef, GameObjectManagerOptions options,
            CancellationToken token);
        
        [Obsolete("User GameObjectAssetRefT<T> instead")]
        UniTask<IGameObjectManagerT<T>> Checkout<T>(GameObjectAssetRef assetRef,
            GameObjectManagerOptions options, CancellationToken token) where T : Component;
        
        UniTask<IGameObjectManagerT<T>> Checkout<T>(GameObjectAssetRefT<T> assetRef,
            GameObjectManagerOptions options, CancellationToken token) where T : Component;
    }
}
#nullable disable