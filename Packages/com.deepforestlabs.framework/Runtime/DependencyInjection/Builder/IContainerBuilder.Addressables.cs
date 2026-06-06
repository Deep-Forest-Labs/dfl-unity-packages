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
        IContainerBuilder AddDownload(GameObjectAssetRef assetRef);
        IContainerBuilder AddScene(SceneAssetRef key);
        IContainerBuilder AddAudioClip(AudioClipAssetRef assetRef);
        IContainerBuilder AddMesh(MeshAssetRef assetRef);
        IContainerBuilder AddSprite(SpriteAssetRef assetRef);
        IContainerBuilder AddAtlasedSprite(AtlasedSpriteAssetRef assetRef);
        IContainerBuilder AddSpriteAtlas(SpriteAtlasAssetRef assetRef);
        IContainerBuilder AddTextures2D(Texture2DAssetRef assetRef);
    }
}
#nullable disable
