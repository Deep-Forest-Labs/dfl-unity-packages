#nullable enable
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace DeepForestLabs.Assets.Resource
{
    internal sealed partial class ResourcesManager
    {
        private sealed class SceneAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public Scene Scene { get; }
            public int Count { get; private set; }

            public SceneAssetHandle(ResourcesManager manager, string resourcesPath, Scene scene)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                Scene = scene;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseScene(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }

        private sealed class AudioClipAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public AudioClip AudioClip { get; }
            public int Count { get; private set; }

            public AudioClipAssetHandle(ResourcesManager manager, string resourcesPath, AudioClip audioClip)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                AudioClip = audioClip;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseAudioClip(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }

        private sealed class MeshAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public Mesh Mesh { get; }
            public int Count { get; private set; }

            public MeshAssetHandle(ResourcesManager manager, string resourcesPath, Mesh mesh)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                Mesh = mesh;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseMesh(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }

        private sealed class SpriteAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public Sprite Sprite { get; }
            public int Count { get; private set; }

            public SpriteAssetHandle(ResourcesManager manager, string resourcesPath, Sprite sprite)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                Sprite = sprite;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseSprite(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }
        
        private sealed class SpriteAtlasAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public SpriteAtlas SpriteAtlas { get; }
            public int Count { get; private set; }

            public SpriteAtlasAssetHandle(ResourcesManager manager, string resourcesPath,
                SpriteAtlas spriteAtlas)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                SpriteAtlas = spriteAtlas;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseSpriteAtlas(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }

        private sealed class Texture2DAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public Texture2D Texture { get; }
            public int Count { get; private set; }

            public Texture2DAssetHandle(ResourcesManager manager, string resourcesPath, Texture2D texture)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                Texture = texture;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseTexture2D(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }

        private sealed class ScriptableObjectAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public ScriptableObject ScriptableObject { get; }

            public int Count { get; private set; }

            public ScriptableObjectAssetHandle(ResourcesManager manager, string resourcesPath,
                ScriptableObject scriptableObject)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                ScriptableObject = scriptableObject;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseScriptableObject(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }

        private sealed class GameObjectAssetHandle
        {
            private ResourcesManager Manager { get; }
            public string ResourcesPath { get; }
            public GameObject Prefab { get; }

            public int Count { get; private set; }

            public GameObjectAssetHandle(ResourcesManager manager, string resourcesPath, GameObject prefab)
            {
                Manager = manager;
                ResourcesPath = resourcesPath;
                Prefab = prefab;
                Count = 0;
            }

            public void Push(CancellationToken token)
            {
                Count++;
                token.Register(Pop);
            }

            private void Pop()
            {
                Count--;
                if (Count == 0)
                {
                    Manager.ReleaseGameObject(this);
                }

                Count = Mathf.Max(0, Count);
            }
        }
    }
}
#nullable disable