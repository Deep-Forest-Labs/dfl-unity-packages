#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeepForestLabs
{
    public interface IGameObjectManager
    {
        GameObject? Prefab { get; }
        
        GameObjectAssetRef AssetRef { get; }
        
        UniTask<GameObject> Checkout(CancellationToken token);
        UniTask<GameObject> Checkout(Transform? parent, CancellationToken token);
        UniTask<GameObject> Checkout(Transform? parent, bool worldPositionStays, CancellationToken token);
        
        UniTask<GameObject> Create(CancellationToken token);
        UniTask<GameObject> Create(Transform? parent, CancellationToken token);
        UniTask<GameObject> Create(Transform? parent, bool worldPositionStays, CancellationToken token);
    }

    public interface IGameObjectManagerT<T> where T : Component
    {
        T? Prefab { get; }
        
        GameObjectAssetRefT<T> AssetRef { get; }
        
        UniTask<T> Checkout(CancellationToken token);
        UniTask<T> Checkout(Transform? parent, CancellationToken token);
        UniTask<T> Checkout(Transform? parent, bool worldPositionStays, CancellationToken token);
        
        UniTask<T> Create(CancellationToken token);
        UniTask<T> Create(Transform? parent, CancellationToken token);
        UniTask<T> Create(Transform? parent, bool worldPositionStays, CancellationToken token);
    }
}
#nullable disable