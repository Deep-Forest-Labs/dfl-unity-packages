#nullable enable
using UnityEngine;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder
    {
        IContainerBuilder AddDownload(SceneAssetRef assetRef);
        IContainerBuilder AddDownload(AudioClipAssetRef assetRef);
        IContainerBuilder AddDownload(MeshAssetRef assetRef);
        IContainerBuilder AddDownload(SpriteAssetRef assetRef);
        IContainerBuilder AddDownload(AtlasedSpriteAssetRef assetRef);
        IContainerBuilder AddDownload(SpriteAtlasAssetRef assetRef);
        IContainerBuilder AddDownload(Texture2DAssetRef assetRef);
        IContainerBuilder AddDownload<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ScriptableObject;
        IContainerBuilder AddDownload(GameObjectAssetRef assetRef);
        IContainerBuilder AddDownload<T>(GameObjectAssetRefT<T> assetRef) where T : Component;

        IContainerBuilder AddScene(SceneAssetRef key);
        IContainerBuilder AddScene<T>(SceneAssetRef key) where T : class;
        IContainerBuilder AddScene<T1, T2>(SceneAssetRef assetRef) where T1 : class where T2 : class;
        IContainerBuilder AddAudioClip(AudioClipAssetRef assetRef);
        IContainerBuilder AddMesh(MeshAssetRef assetRef);
        IContainerBuilder AddSprite(SpriteAssetRef assetRef);
        IContainerBuilder AddAtlasedSprite(AtlasedSpriteAssetRef assetRef);
        IContainerBuilder AddSpriteAtlas(SpriteAtlasAssetRef assetRef);
        IContainerBuilder AddTextures2D(Texture2DAssetRef assetRef);
        IContainerBuilder AddScriptableObject<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ScriptableObject;
        IContainerBuilder AddGameObjectManager(GameObjectAssetRef assetRef, GameObjectManagerOptions option);
        IContainerBuilder AddGameObjectManager<T>(GameObjectAssetRefT<T> assetRef, GameObjectManagerOptions option) where T : Component;
        IContainerBuilder AddScopedGameObject(GameObjectAssetRef assetRef, Transform? parent, bool worldPositionStays = true);
        IContainerBuilder AddScopedGameObject<T>(GameObjectAssetRefT<T> assetRef, Transform? parent, bool worldPositionStays = true) where T : Component;
    }
}
#nullable disable