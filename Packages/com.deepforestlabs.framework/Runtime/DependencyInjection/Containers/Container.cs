#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DeepForestLabs.Assets.Addressables;
using DeepForestLabs.Assets.Resource;
using DeepForestLabs.Common;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using AsyncResolver = System.Func<DeepForestLabs.IDiCollection, System.Threading.CancellationToken, Cysharp.Threading.Tasks.UniTask<object>>;
using Resolver = System.Func<DeepForestLabs.IDiCollection, object>;

namespace DeepForestLabs
{
    internal sealed partial class Container : IContainer
    {
        private static Container? Root { get; set; }
        
        internal readonly CancellationTokenSource _scope;
        internal readonly string _name;
        internal readonly GameObject _gameObject;
        internal readonly Container _parent;
        internal readonly bool _isFactory;
        internal readonly List<Container> _children = new();
        internal readonly List<object> _transients = new();
        internal readonly ResourcesManager _resourcesManager;
        internal AddressablesManager? _addressablesManager;
        
        internal readonly Dictionary<Type, object> _singletons = new();
        internal Dictionary<Type, object>? _scoped = default;
        internal Dictionary<Type, Resolver>? _transientsResolvers = default;
        internal Dictionary<Type, AsyncResolver>? _asyncTransientResolvers = default;

        public string Name => _name;
        
        public IContainer Parent => _parent;

        public Transform Transform => _gameObject.transform;

        public IReadOnlyList<IContainer> Children => _children;
        
        public CancellationToken Scope => _scope.IsCancellationRequested ? new CancellationToken(true) : _scope.Token;
        
        public bool IsDisposed => _scope.IsCancellationRequested;

        internal Action<IContainer>? _onBuildComplete;

        event Action<IContainer>? IContainer.OnBuildComplete
        {
            add => _onBuildComplete += value;
            remove => _onBuildComplete -= value;
        }

        public event Action<IContainer>? OnPreDispose;

        internal Container(string name, GameObject gameObject, CancellationToken token)
        {
            _scope = CancellationTokenSource.CreateLinkedTokenSource(token);
            _name = name;
            _gameObject = gameObject;
            _parent = this;
            _resourcesManager = new ResourcesManager(this, _scope.Token);
            _addressablesManager = null!;
            
            _singletons.Add(typeof(IContainer), this);
            _singletons.Add(typeof(CancellationToken), _scope.Token);
            _singletons.Add(typeof(ResourcesManager), _resourcesManager);
            _singletons.Add(typeof(GameObject), _gameObject);
            _singletons.Add(typeof(Transform), _gameObject.transform);
            _singletons.Add(typeof(AsyncApplicationFocusTrigger), _gameObject.AddComponent<AsyncApplicationFocusTrigger>());
            _singletons.Add(typeof(AsyncApplicationPauseTrigger), _gameObject.AddComponent<AsyncApplicationPauseTrigger>());
        }

        internal Container(string name, Container parent, GameObject? gameObject = null, bool isFactory = false)
        {
            _scope = CancellationTokenSource.CreateLinkedTokenSource(parent.Scope);
            _name = name;
            _gameObject = gameObject == null ? parent._gameObject : gameObject;
            _parent = parent;
            _parent.LinkChild(this, _scope.Token);
            _isFactory = isFactory;
            _resourcesManager = parent._resourcesManager;
            _addressablesManager = parent._addressablesManager;

            _singletons.Add(typeof(IContainer), this);
            _singletons.Add(typeof(CancellationToken), _scope.Token);
            _singletons.Add(typeof(ResourcesManager), _resourcesManager);
            _singletons.Add(typeof(GameObject), _gameObject);
            _singletons.Add(typeof(Transform), _gameObject.transform);
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().AsUniTask().Forget();
        }

        public async ValueTask DisposeAsync()
        {
            if (_scope.IsCancellationRequested)
            {
                return;
            }
            
            InternalUtils.VerboseLog(_name, "Disposing", ContainerLogFlag.DisposingContainer);
            
            // Used to disconnect/pop of ServiceLocator stack.
            OnPreDispose?.Invoke(this);
            
            // Cancel + Dispose container scope
            _scope.Cancel();
            _scope.Dispose();

            // Children
            List<UniTask>? tasks = null;
            foreach (Container? child in _children.ToArray())
            {
                tasks ??= new();
                tasks.Add(SafeDispose(child));
            }
            _children.Clear();
            if (tasks != null)
            {
                await UniTask.WhenAll(tasks);
                tasks.Clear();
            }
            
            // Singletons
            _singletons.Clear();
            
            // Scoped
            if (_scoped != null)
            {
                foreach (object? value in _scoped.Values)
                {
                    if (value is IDisposable disposable)
                    {
                        SafeDispose(disposable);
                    }
                    else if (value is IAsyncDisposable asyncDisposable)
                    {
                        tasks ??= new List<UniTask>();
                        tasks.Add(SafeDisposeAsync(asyncDisposable));
                    }
                    else
                    {
                        //InternalUtils.VerboseLog(_name, ZString.Format("Destroying {0}.", InternalUtils.FormatTypeName(value.GetType())),ContainerLogFlag.Disposing);
                    }
                }
                _scoped.Clear();
            }

            // Transients
            foreach (object value in _transients)
            {
                if (value is IDisposable disposable)
                {
                    SafeDispose(disposable);
                }
                else if (value is IAsyncDisposable asyncDisposable)
                {
                    tasks ??= new List<UniTask>();
                    tasks.Add(SafeDisposeAsync(asyncDisposable));
                }
                else
                {
                    //InternalUtils.VerboseLog(_name, ZString.Format("Destroying {0}.", InternalUtils.FormatTypeName(value.GetType())),ContainerLogFlag.Disposing);
                }
            }
            _transients.Clear();

            if (tasks != null)
            {
                await UniTask.WhenAll(tasks);
                tasks.Clear();
            }
            
            if (_addressablesManager != null && _parent._addressablesManager == null)
            {
                try
                {
                    await _addressablesManager.DisposeAsync();
                }
                catch (Exception e)
                {
                    Log.DevException(e, "Exception disposing AddressablesManager in Container: {0}", _name);
                }
            }
            _addressablesManager = null;

            if (this == Root)
            {
                try
                {
                    await _resourcesManager.DisposeAsync();
                }
                catch (Exception e)
                {
                    Log.DevException(e, "Exception disposing ResourcesManager in Container: {0}", _name);
                }
            }

            InternalUtils.VerboseLog(_name, "Disposed", ContainerLogFlag.DisposingContainer);
        }
        
                
        private async UniTask SafeDispose(Container? child)
        {
            if (child == null)
            {
                return;
            }
                    
            try
            {
                await child.DisposeAsync().AsTask().AsUniTask();
            }
            catch (Exception e)
            {
                Log.DevException(e, "An error occured disposing child container {0}.\n{1} - {2}\n{3}",
                    (child).Name, e.GetType().Name, e.Message, e.StackTrace);
            }
        }
        
        private void SafeDispose(IDisposable? disposable)
        {
            if (disposable == null)
            {
                return;
            }
            
            try
            {
                InternalUtils.VerboseLog(_name, ZString.Format("Disposing {0}.", InternalUtils.FormatTypeName(disposable.GetType())),
                    ContainerLogFlag.Disposing);
                
                disposable.Dispose();
            }
            catch (Exception e)
            {
                Log.DevException(e, "An error occured disposing  {0}.\n{1} - {2}\n{3}",
                    InternalUtils.FormatTypeName(disposable.GetType()), e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        private async UniTask SafeDisposeAsync(IAsyncDisposable? disposable)
        {
            if (disposable == null)
            {
                return;
            }
            
            try
            {
                InternalUtils.VerboseLog(_name, ZString.Format("Disposing {0}.", InternalUtils.FormatTypeName(disposable.GetType())), ContainerLogFlag.Disposing);
                await disposable.DisposeAsync();
                InternalUtils.VerboseLog(_name, ZString.Format("Disposing {0} complete.", InternalUtils.FormatTypeName(disposable.GetType())), ContainerLogFlag.Disposing);
            }
            catch (Exception e)
            {
                Log.DevException(e, "An error occured disposing {0}.\n{1} - {2}", InternalUtils.FormatTypeName(disposable.GetType()),
                    e.GetType().Name, e.Message);
            }
        }
        
        public IContainerBuilder CreateChild(string name)
        {
            return new ContainerBuilder(name, this, Root!._gameObject, false);
        }
        
        public async UniTask EnableAddressables(string withCdn, CancellationToken token)
        {
            Log.Assert(!string.IsNullOrEmpty(withCdn), "!string.IsNullOrEmpty(withCdn)");
            Log.Assert(!Variables.IsRuntimeVariablesConfigured, "!Variables.IsRuntimeVariablesConfigured");
            Variables.Configure(withCdn);
            _scope.Token.Register(Variables.Clear);

            Log.Debug("[AddressablesService] Remote Load Path: {0}", Variables.RemoteLoadPath);
            Log.Debug("[AddressablesService] Local Load Path: {0}", Variables.LocalLoadPath);
#if UNITY_EDITOR
            Log.Debug("[AddressablesService] Builder Index: {0}", BuildSettings.Instance.Addressables.ActivePlayModeIndex);
#endif

            // Check to see if the config Remote Load Path has been set otherwise we can't load asset bundles.
            if (string.IsNullOrEmpty(Variables.RemoteLoadPath))
            {
                throw new GameException("AddressablesRuntimeVariables.RemoteLoadPath is not set.");
            }
#if UNITY_EDITOR
            else if (string.IsNullOrEmpty(Variables.LocalLoadPath))
            {
                throw new GameException("AddressablesRuntimeVariables.LocalLoadPath is not set.");
            }
#endif
            
            _addressablesManager = new AddressablesManager(this, _scope.Token);
            await _addressablesManager.Initialize(_scope.Token)
                    .AttachExternalCancellation(token);
        }

        internal void LinkChild(Container child, CancellationToken scope)
        {
            _children.Add(child);
            scope.Register(() => _children.Remove(child));
        }

#if UNITY_EDITOR
#endif

        // Not used currently, could be nice for dev builds
        // private bool IsRegistered(Type t)
        // {
        //     return (_singletonResolvers != null && _singletonResolvers.ContainsKey(t)) ||
        //         (_asyncSingletonResolvers != null && _asyncSingletonResolvers.ContainsKey(t))||
        //         (_singletons != null && _singletons.ContainsKey(t))||
        //         (_scopedResolvers != null && _scopedResolvers.ContainsKey(t))||
        //         (_asyncScopedResolvers != null && _asyncScopedResolvers.ContainsKey(t))||
        //         (_aliases != null && _aliases.ContainsKey(t))||
        //         (_transientsResolvers != null && _transientsResolvers.ContainsKey(t))||
        //         (_asyncTransientResolvers != null && _asyncTransientResolvers.ContainsKey(t));
        // }

        public override string ToString()
        {
            return _name;
        }

        public async UniTask UnloadUnusedResources(CancellationToken token)
        {
            await _resourcesManager.UnloadUnusedAssets(token);
        }
    }
}
#nullable disable