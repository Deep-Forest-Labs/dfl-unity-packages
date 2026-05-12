#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Assets.Addressables;
using UnityEngine;

namespace DeepForestLabs
{
    internal sealed class GameObjectManager : IGameObjectManager, IDisposable
    {
        private readonly Container _container;
        private readonly CancellationTokenSource _scope;
        private readonly GameObjectAssetRef _assetRef;
        private readonly GameObjectManagerOptions _options;
        private readonly Queue<GameObject> _pooled = new();
        private readonly HashSet<GameObject> _checkedOut = new();
        private readonly HashSet<GameObject> _created = new();

        private CancellationTokenSource? _loadScope;
        private UniTaskCompletionSource? _loadCompletionSource;
        private GameObject? _prefab;
        public GameObject? Prefab => _prefab;

        public GameObjectAssetRef AssetRef => _assetRef;

        private int _referenceCount;

        internal event Action<GameObject>? OnLoad;
        internal event Action? OnUnloaded;

        internal GameObjectManager(Container container, GameObjectAssetRef assetRef,
            GameObjectManagerOptions options)
        {
            _scope = new CancellationTokenSource();
            _container = container;
            _assetRef = assetRef;
            _options = options;
        }

        public void Dispose()
        {
            Unload();
            _scope.Cancel();
            _scope.Dispose();
        }

        internal void Push(CancellationToken scope)
        {
            Interlocked.Increment(ref _referenceCount);
            scope.Register(static s => ((GameObjectManager)s!).Pop(), this);
        }

        internal async UniTask LoadRequired(CancellationToken token)
        {
            if (_options.DownloadOptions.HasFlag(GameObjectManagerDownloadOptions.Required))
            {
                await _container.Download(_assetRef, _scope.Token)
                    .AttachExternalCancellation(token);
            }

            if (_options.LoadOptions.HasFlag(GameObjectManagerLoadOptions.Required))
            {
                await Load(_scope.Token)
                    .AttachExternalCancellation(token);
            }

            LoadInBackground(_scope.Token).Forget();
        }

        private async UniTaskVoid LoadInBackground(CancellationToken token)
        {
            if (_options.DownloadOptions.HasFlag(GameObjectManagerDownloadOptions.Background))
            {
                await _container.Download(_assetRef, token);
            }

            if (_options.LoadOptions.HasFlag(GameObjectManagerLoadOptions.Background))
            {
                await Load(token);
            }
        }

        public UniTask<GameObject> Create(CancellationToken token) => Create(null, true, token);

        public UniTask<GameObject> Create(Transform? parent, CancellationToken token) =>
            Create(parent, true, token);

        public async UniTask<GameObject> Create(Transform? parent, bool worldPositionStays, CancellationToken token)
        {
            Log.Assert(_referenceCount != 0, "ReferenceCount != 0");

            if (!_options.UseCreate)
            {
                return await Checkout(parent, worldPositionStays, token);
            }

            if (_prefab == null)
            {
                await Load(token);
            }

            Log.Assert(_prefab != null, "_prefab != null");
            GameObject instance = GameObject.Instantiate(_prefab, parent, worldPositionStays);
            instance.name = _prefab.name;
            instance.hideFlags = HideFlags.DontSave;
            _created.Add(instance);
#if UNITY_EDITOR
            if (_assetRef._mode == AssetRefMode.Addressables)
            {
                AddressablesManager.onInstantiated?.Invoke(instance);
            }
#endif

            void Destroy()
            {
                if (instance != null)
                {
                    _created.Remove(instance);
                    DestroyInstance(instance);

                    if (_options.AutoUnload && !_options.Prewarm && _checkedOut.Count == 0 && _created.Count == 0)
                    {
                        AutoUnload(_scope.Token).Forget();
                    }
                }
            }

            token.Register(Destroy);
            return instance;
        }

        public UniTask<GameObject> Checkout(CancellationToken token) => Checkout(null, true, token);

        public UniTask<GameObject> Checkout(Transform? parent, CancellationToken token) =>
            Checkout(parent, true, token);

        public async UniTask<GameObject> Checkout(Transform? parent, bool worldPositionStays,
            CancellationToken token)
        {
            Log.Assert(_referenceCount != 0, "ReferenceCount != 0");
            if (_options.UseCreate)
            {
                return await Create(parent, worldPositionStays, token);
            }

            GameObject instance;
            if (_pooled.Count > 0)
            {
                instance = _pooled.Dequeue();
                instance.transform.SetParent(parent, worldPositionStays);
            }
            else
            {
                if (_prefab == null)
                {
                    await Load(token);
                }

                Log.Assert(_prefab != null, "_prefab != null");
                instance = GameObject.Instantiate(_prefab, parent, worldPositionStays);
#if UNITY_EDITOR
                AddressablesManager.onInstantiated?.Invoke(instance);
#endif
            }

            instance.SetActive(true);

            _checkedOut.Add(instance);

            void CheckIn(GameObject checkInInstance)
            {
                if (checkInInstance != null)
                {
                    if (!_checkedOut.Remove(checkInInstance))
                    {
                        DestroyInstance(checkInInstance);
                    }
                    else if (_options.MaxPoolSize > 0 && _pooled.Count >= _options.MaxPoolSize)
                    {
                        DestroyInstance(checkInInstance);
                    }
                    else
                    {
                        _pooled.Enqueue(checkInInstance);
                        checkInInstance.SetActive(false);
                        checkInInstance.transform.SetParent(_container.IsDisposed ? null : _container.Transform, worldPositionStays);
                    }
                }

                if (_options.AutoUnload && !_options.Prewarm && _checkedOut.Count == 0 && _created.Count == 0)
                {
                    AutoUnload(_scope.Token).Forget();
                }
            }

            token.Register(() => CheckIn(instance));

            return instance;
        }

        private void Pop()
        {
            Interlocked.Decrement(ref _referenceCount);
            if (_referenceCount == 0)
            {
                Unload();
            }
            _referenceCount = Mathf.Max(0, _referenceCount);
        }

        private async UniTask Load(CancellationToken token)
        {
            if (_loadCompletionSource == null)
            {
                _loadCompletionSource = new();
                _loadScope?.Cancel();
                _loadScope?.Dispose();
                _loadScope = CancellationTokenSource.CreateLinkedTokenSource(_scope.Token);
                LoadInternal(_loadScope.Token).Forget();
            }

            await _loadCompletionSource.Task.AttachExternalCancellation(token);
        }

        private async UniTaskVoid LoadInternal(CancellationToken token)
        {
            if (_prefab != null)
            {
                Log.Assert(_loadScope != null, "_loadScope == null");
                return;
            }

            Log.Assert(_prefab == null, "_loadedHandle == null");
            try
            {
                _prefab = await _container.CheckoutPrefab(_assetRef, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _loadCompletionSource?.TrySetException(e);
                Unload();
                return;
            }
            token.Register(() => _prefab = null);

            if (!_options.UseCreate)
            {
                int prewarmCount = _options.ResolvePrewarmCount();
                for (int i = 0; i < prewarmCount; i++)
                {
                    GameObject prewarm = GameObject.Instantiate(_prefab, _container.Transform);
#if UNITY_EDITOR
                    if (_assetRef._mode == AssetRefMode.Addressables)
                    {
                        AddressablesManager.onInstantiated?.Invoke(prewarm);
                    }
#endif
                    prewarm.SetActive(false);
                    _pooled.Enqueue(prewarm);
                }
            }

            OnLoad?.Invoke(_prefab);
            _loadCompletionSource?.TrySetResult();
        }

        private async UniTaskVoid AutoUnload(CancellationToken token)
        {
            await UniTask.NextFrame(token);

            if (_checkedOut.Count == 0 && _created.Count == 0)
            {
                Unload();
            }
        }

        private static void DestroyInstance(GameObject instance)
        {
            if (instance == null) return;
            if (Application.isEditor)
            {
                GameObject.DestroyImmediate(instance);
            }
            else
            {
                GameObject.Destroy(instance);
            }
        }

        private void Unload()
        {
            foreach (GameObject pooled in _pooled)
                DestroyInstance(pooled);
            _pooled.Clear();

            foreach (GameObject checkedOut in _checkedOut)
                DestroyInstance(checkedOut);
            _checkedOut.Clear();

            foreach (GameObject created in _created)
                DestroyInstance(created);
            _created.Clear();

            _loadScope?.Cancel();
            _loadScope?.Dispose();
            _loadScope = null;
            _loadCompletionSource = null;
            OnUnloaded?.Invoke();
        }
    }
    
    internal sealed class GameObjectManagerT<T> : IGameObjectManagerT<T>
        where T : Component
    {
        private readonly GameObjectManager _proxy;

        public T? Prefab { get; private set; }
        
        public GameObjectAssetRefT<T> AssetRef => (_proxy.AssetRef as GameObjectAssetRefT<T>)!;

        internal GameObjectManagerT(GameObjectManager proxy)
        {
            _proxy = proxy;
            proxy.OnLoad += OnLoaded;
            proxy.OnUnloaded += OnUnload;
        }

        public UniTask<T> Create(CancellationToken token) => Create(null, true, token);
        public UniTask<T> Create(Transform? parent, CancellationToken token) => Create(parent, true, token);

        public async UniTask<T> Create(Transform? parent, bool worldPositionStays, CancellationToken token)
        {
            GameObject? gameObject = await _proxy.Create(parent, worldPositionStays, token);
            return gameObject.GetComponent<T>();
        }

        internal void Push(CancellationToken token)
        {
            _proxy.Push(token);
        }

        public UniTask<T> Checkout(CancellationToken token) => Checkout(null, true, token);
        public UniTask<T> Checkout(Transform? parent, CancellationToken token) => Checkout(parent, true, token);

        public async UniTask<T> Checkout(Transform? parent, bool worldPositionStays, CancellationToken token)
        {
            GameObject? gameObject = await _proxy.Checkout(parent, worldPositionStays, token);
            T? instance = gameObject.GetComponent<T>();
            if (instance is IManagedGameObjectCallbackReceiver receiver)
            {
                receiver.OnCheckedOut();
                token.Register(() =>
                {
                    if (gameObject != null)
                    {
                        receiver.OnCheckedIn();
                    }
                });
            }

            return instance;
        }

        private void OnLoaded(GameObject go)
        {
            Prefab = go.GetComponent<T>();
        }

        private void OnUnload()
        {
            Prefab = null;
        }
    }
}