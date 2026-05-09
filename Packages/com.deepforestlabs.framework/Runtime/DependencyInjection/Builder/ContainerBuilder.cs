#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;
using DeepForestLabs.Factories;
using UnityEngine;
using AsyncResolver = System.Func<DeepForestLabs.IDiCollection, System.Threading.CancellationToken, Cysharp.Threading.Tasks.UniTask<object>>;
using Resolver = System.Func<DeepForestLabs.IDiCollection, object>;

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder : IContainerBuilder, IParentContainerBuilder
    {
        private readonly List<UniTask> _tasks = new();
        private readonly List<UniTask<DiException?>> _injectTasks = new();
        internal readonly Container _container;

        private List<UniTask>? _downloads = default;
        private Dictionary<Type, Resolver>? _singletonResolvers = default;
        private Dictionary<Type, AsyncResolver>? _asyncSingletonResolvers = default;
        private Dictionary<Type, Resolver>? _scopedResolvers = default;
        private Dictionary<Type, AsyncResolver>? _asyncScopedResolvers = default;
        private Dictionary<object, AsyncResolver>? _builderFactoryResolvers = default;
        private Dictionary<(int index, object assetRef), AsyncResolver>? _containerFactoryResolvers = default;
        private List<(ContainerFactory factory, ContainerBuilder builder)>? _containerFactories = default;
        private Dictionary<Type, Type>? _aliases = default;

        public string Name => _container._name;
        public IContainer Parent => _container._parent;

        public CancellationToken Scope => _container.Scope;
        public IDiCollection Collection => _container;

        public event Action<IContainer>? OnPreInitialize;
        
        public event Action<IContainer>? OnBuildComplete
        {
            add => _container._onBuildComplete += value;
            remove => _container._onBuildComplete -= value;
        }

        internal ContainerBuilder(string name, GameObject gameObject, CancellationToken token)
        {
            _container = new Container(name, gameObject, token);
            InternalUtils.VerboseLog(Name, "Created", ContainerLogFlag.CreatedContainer);
        }
        
        internal ContainerBuilder(string name, Container parent, GameObject gameObject, bool isFactory)
        {
            _container = new Container(name, parent, gameObject, isFactory);
            InternalUtils.VerboseLog(Name, "Created", ContainerLogFlag.CreatedContainer);
        }

        public async UniTask<IContainer> Build(CancellationToken token)
        {
            try
            {
                return await BuildInternal(_container.Scope)
                    .AttachExternalCancellation(token);
            }
            catch (Exception)
            {
                await _container.DisposeAsync();
                throw;
            }
            finally
            {
                _tasks.Clear();
                _injectTasks.Clear();
                _downloads = null;
                _singletonResolvers = null;
                _asyncSingletonResolvers = null;
                _scopedResolvers = null;
                _asyncScopedResolvers = null;
                _builderFactoryResolvers = null;
                _containerFactoryResolvers = null;
                _containerFactories = null;
                _aliases = null;
            }
        }
        
        private async UniTask<IContainer> BuildInternal(CancellationToken token)
        {
            Log.Assert(!_container._isFactory, "!_container._isFactory");

            ValidatePreBuild();

            BuildStarted();

            await Resolve(token);

            Arrange();

            await Inject(token);

            PreInitialize();

            await Initialize(token);

            ValidatePostBuild();

            BuildComplete();

            return _container;
        }
        
        public object Get(Type type)
        {
            return _container.Get(type);
        }

        public T Get<T>()
        {
            return _container.Get<T>();
        }

        public T? Find<T>()
        {
            return _container.Find<T>();
        }

        public bool TryGet<T>([NotNullWhen(true)] out T? instance)
        {
            return _container.TryGet(out instance);
        }

        private UniTask Resolve(CancellationToken token)
        {
            return ResolveRecursive(this, null, token);
        }
        
        private async UniTask ResolveRecursive(ContainerBuilder? parent, ContainerFactory? factory, CancellationToken token)
        {
            if (parent != null && factory != null)
            {
                BuildStarted();
                factory.Resolve(parent, this);
            }
            
            List<ContainerBuilderFactory>? containerBuilders = null;

            #region Downloading

            if (_downloads != null)
            {
                await UniTask.WhenAll(_downloads);
                _downloads.Clear();
            }

            #endregion

            #region Loading
            if (_builderFactoryResolvers != null)
            {
                foreach (KeyValuePair<object, AsyncResolver> pair in _builderFactoryResolvers)
                {
                    AsyncResolver resolver = pair.Value;
                    
                    InternalUtils.VerboseLog(Name, ZString.Format("Started Loading {0} {1}.", nameof(ContainerBuilderFactory), pair.Key.ToString()), ContainerLogFlag.Loading);

                    void Resolved(object result)
                    {
                        Log.Assert(result is ContainerBuilderFactory, "result is ContainerBuilderFactory");
                        ContainerBuilderFactory builderFactory = (result as ContainerBuilderFactory)!;
                        
                        InternalUtils.VerboseLog(Name, ZString.Format("Finished Loading {0} {1}.", nameof(ContainerBuilderFactory), pair.Key.ToString()), ContainerLogFlag.Loading);
                        containerBuilders ??= new List<ContainerBuilderFactory>();
                        containerBuilders.Add(builderFactory);
                    }

                    _tasks.Add(resolver(_container, _container._scope.Token)
                        .ContinueWith(Resolved));
                }

                _builderFactoryResolvers.Clear();
                _builderFactoryResolvers = null;
            }
            
            if (_containerFactoryResolvers != null)
            {
                foreach (KeyValuePair<(int index, object assetRef), AsyncResolver> pair in _containerFactoryResolvers)
                {
                    int index = pair.Key.index;
                    Log.Assert(_containerFactories != null, "_containerFactories != null");
                    Log.Assert(index <= _containerFactories.Count, "{0} <= {1}", index, _containerFactories.Count);
                    
                    AsyncResolver resolver = pair.Value;
                    
                    InternalUtils.VerboseLog(Name, ZString.Format("Started Loading {0} {1}.", nameof(ContainerFactory), pair.Key.assetRef.ToString()), ContainerLogFlag.Loading);

                    void Resolved(object result)
                    {
                        Log.Assert(result is ContainerFactory, "result is ContainerFactory");
                        InternalUtils.VerboseLog(Name, ZString.Format("Finished Loading {0} {1}.", nameof(ContainerFactory), pair.Key.assetRef.ToString()), ContainerLogFlag.Loading);
                        
                        ContainerFactory containerFactory = (result as ContainerFactory)!;
                        string name = ZString.Format("{0}[{1}]", containerFactory.Name, index);
                        ContainerBuilder builder = new ContainerBuilder(name, _container, _container._gameObject, true);
                        _containerFactories[index] = (containerFactory, builder);
                    }

                    _tasks.Add(resolver(_container, _container._scope.Token)
                        .ContinueWith(Resolved));
                }

                _containerFactoryResolvers.Clear();
                _containerFactoryResolvers = null;
            }

            if (_tasks.Count > 0)
            {
                await UniTask.WhenAll(_tasks);
                _tasks.Clear();
            }

            #endregion

            #region Construction
            _tasks.Clear();
            
            if (containerBuilders != null)
            {
                _injectTasks.Clear();
                foreach (ContainerBuilderFactory containerBuilderFactory in containerBuilders)
                {
                    _injectTasks.Add(_container.InjectRecursive(_container, containerBuilderFactory, 0, _container._transients, token));
                }

                await UniTask.WhenAll(_injectTasks);
                _injectTasks.Clear();

                foreach (ContainerBuilderFactory containerBuilderFactory in containerBuilders)
                {
                    _container._singletons.Add(containerBuilderFactory.GetType(), containerBuilderFactory);
                    containerBuilderFactory.AddToBuilder(this);
                }
                containerBuilders.Clear();
            }

            if (_singletonResolvers != null)
            {
                foreach (KeyValuePair<Type, Resolver> pair in _singletonResolvers)
                {
                    InternalUtils.VerboseLog(Name,
                        ZString.Format("Adding Singleton {0}.", InternalUtils.FormatTypeName(pair.Key)),
                        ContainerLogFlag.Instantiation);
                    _container._singletons.Add(pair.Key, pair.Value(_container));
                }

                _singletonResolvers.Clear();
                _singletonResolvers = null;
            }
            
            if (_asyncSingletonResolvers != null)
            {
                foreach (KeyValuePair<Type, AsyncResolver> pair in _asyncSingletonResolvers)
                {
                    InternalUtils.VerboseLog(Name,
                        ZString.Format("Adding Singleton {0}.", InternalUtils.FormatTypeName(pair.Key)),
                        ContainerLogFlag.Instantiation);
                    
                    AsyncResolver resolver = pair.Value;
                    
                    UniTask task = resolver(_container, _container._scope.Token)
                        .ContinueWith(asyncInstance =>
                        {
                            Log.Assert(asyncInstance != null, "asyncInstance != null");
                            _container._singletons.Add(pair.Key, asyncInstance);
                        });
                    _tasks.Add(task);
                }

                _asyncSingletonResolvers.Clear();
                _asyncSingletonResolvers = null;
            }
            
            if (_scopedResolvers != null)
            {
                foreach (KeyValuePair<Type, Resolver> pair in _scopedResolvers)
                {
                    InternalUtils.VerboseLog(Name,
                        ZString.Format("Constructing Scoped {0}.", InternalUtils.FormatTypeName(pair.Key)),
                        ContainerLogFlag.Instantiation);
                    _container._scoped ??= new Dictionary<Type, object>();
                    _container._scoped.Add(pair.Key, pair.Value(_container));
                }

                _scopedResolvers.Clear();
                _scopedResolvers = null;
            }

            if (_asyncScopedResolvers != null)
            {
                foreach (KeyValuePair<Type, AsyncResolver> pair in _asyncScopedResolvers)
                {
                    InternalUtils.VerboseLog(Name,
                        ZString.Format("Constructing Scoped {0}.", InternalUtils.FormatTypeName(pair.Key)),
                        ContainerLogFlag.Instantiation);
                    
                    AsyncResolver resolver = pair.Value;
                    
                    UniTask task = resolver(_container, _container._scope.Token)
                        .ContinueWith(asyncInstance =>
                        {
                            Log.Assert(asyncInstance != null, "asyncInstance != null");
                            _container._scoped ??= new Dictionary<Type, object>();
                            _container._scoped.Add(pair.Key, asyncInstance);
                        });
                    _tasks.Add(task);
                }

                _asyncScopedResolvers.Clear();
                _asyncScopedResolvers = null;
            }

            if (_tasks.Count > 0)
            {
                await UniTask.WhenAll(_tasks);
                _tasks.Clear();
            }

            #endregion
            
            #region Map Aliases
            
            // Maps any registered by this container.
            MapAliases();
            
            #endregion

            #region Resolve Child Factories

            if (_containerFactories != null)
            {
                // Inject
                _injectTasks.Clear();
                foreach (ContainerFactory? containerFactory in _containerFactories.Select(x => x.factory).ToHashSet())
                {
                    _container._singletons.Add(containerFactory.GetType(), containerFactory);
                    _injectTasks.Add(_container.InjectRecursive(_container, containerFactory, 0, _container._transients, token));
                }

                await UniTask.WhenAll(_injectTasks);
                _injectTasks.Clear();
                
                // Resolve children
                foreach ((ContainerFactory factory, ContainerBuilder builder) pair in _containerFactories)
                {
                    _tasks.Add(pair.builder.ResolveRecursive(this, pair.factory, token));
                }

                await UniTask.WhenAll(_tasks);
                _tasks.Clear();
            }
            
            #endregion
        }

        private void Arrange()
        {
            ArrangeRecursive(this, null);
        }
        
        private void ArrangeRecursive(ContainerBuilder parent, ContainerFactory? factory)
        {
            if (factory != null)
            {
                factory.Arrange(parent, this, _container._children);
            }

            // Promotion from children
            if (_containerFactories != null)
            {
                foreach ((ContainerFactory factory, ContainerBuilder builder) child in _containerFactories)
                {
                    child.builder.ArrangeRecursive(this, child.factory);
                }
            }
            
            parent.MapAliases();
        }
        
        private void MapAliases()
        {
            if (_aliases != null)
            {
                foreach (KeyValuePair<Type, Type> alias in _aliases.ToArray())
                {
                    if ((_container._scoped != null && _container._scoped.TryGetValue(alias.Value, out object? reference)) ||
                        _container._singletons.TryGetValue(alias.Value, out reference))
                    {
                        if (_container._singletons.TryAdd(alias.Key, reference))
                        {
                            InternalUtils.VerboseLog(Name, ZString.Format("Aliasing {0} -> {1}.",
                                InternalUtils.FormatTypeName(alias.Key),
                                InternalUtils.FormatTypeName(alias.Value)), ContainerLogFlag.Instantiation);
                            _aliases.Remove(alias.Key);
                        }
                        else
                        {
                            throw new DiException(ZString.Format("Duplicate alias to '{0}'",
                                InternalUtils.FormatTypeName(alias.Key)));
                        }
                    }
                    else
                    {
                        throw new DiException(ZString.Format("Failed to locate base type '{0}' to be alias to '{1}'",
                            InternalUtils.FormatTypeName(alias.Value), InternalUtils.FormatTypeName(alias.Key)));
                    }
                }

                _aliases.Clear();
                _aliases = null;
            }
        }

        private UniTask Inject(CancellationToken token)
        {
            return InjectRecursive(_containerFactories, token);
        }
        
        private async UniTask InjectRecursive(List<(ContainerFactory factory, ContainerBuilder builder)>? containerFactories,
            CancellationToken token)
        {
            if (_container._scoped != null)
            {
                _injectTasks.Clear();
                foreach (KeyValuePair<Type, object> pair in _container._scoped)
                {
                    _injectTasks.Add(_container.InjectRecursive(_container, pair.Value, 0, _container._transients, token));
                }
                
                if (_injectTasks.Count > 0)
                {
                    DiException?[]? exceptions = await UniTask.WhenAll(_injectTasks);
                    _injectTasks.Clear();
                    
                    if (exceptions != null && exceptions.Length > 0)
                    {
                        Exception? ex = null;
                        foreach (DiException? exception in exceptions)
                        {
                            if (exception != null)
                            {
                                ex ??= exception;
                            }
                        }

                        if (ex != null)
                        {
                            throw ex;
                        }
                    }
                }
            }
            
            if (containerFactories != null)
            {
                _tasks.Clear();
                // Inject children
                foreach ((ContainerFactory factory, ContainerBuilder builder) pair in containerFactories)
                {
                    ContainerBuilder? builder = pair.builder;
                    _tasks.Add(builder.Inject(token));
                }

                if (_tasks.Count > 0)
                {
                    await UniTask.WhenAll(_tasks);
                }
                _tasks.Clear();
            }
        }

        private UniTask Initialize(CancellationToken token)
        {
            return InitializeRecursive(_containerFactories, token);
        }
        
        private async UniTask InitializeRecursive(List<(ContainerFactory factory, ContainerBuilder builder)>? containerFactories, CancellationToken token)
        {
            if (_container._scoped != null)
            {
                foreach (object scoped in _container._scoped.Values)
                {
                    if (scoped is not IInitializable initializable)
                    {
                        continue;
                    }
#if RELEASE_BUILD
                    _tasks.Add(initializable.Initialize(token));
#else
                    _tasks.Add(InitializeAndLog(Name, initializable, token));
#endif
                }
            }

            foreach (object transient in _container._transients)
            {
                if (transient is not IInitializable initializable)
                {
                    continue;
                }
#if RELEASE_BUILD
                _tasks.Add(initializable.Initialize(token));
#else
                _tasks.Add(InitializeAndLog(Name, initializable, token));
#endif
            }

            if (containerFactories != null)
            {
                // Initialize children
                foreach ((ContainerFactory factory, ContainerBuilder builder) pair in containerFactories)
                {
                    _tasks.Add(pair.builder.Initialize(token));
                }
            }

            InternalUtils.VerboseLog(Name, ZString.Format("Initializing - Start [{0}]", _tasks.Count), ContainerLogFlag.Initialization);

            await UniTask.WhenAll(_tasks);
            _tasks.Clear();
            
            InternalUtils.VerboseLog(Name, "Initializing - Complete", ContainerLogFlag.Initialization);
        }

        private void BuildStarted()
        {
            InternalUtils.VerboseLog(Name, "Building", ContainerLogFlag.BuildingContainer);
        }

        private void PreInitialize()
        {
            OnPreInitialize?.Invoke(_container);
            if (_containerFactories != null)
            {
                foreach ((ContainerFactory factory, ContainerBuilder builder) pair in _containerFactories)
                {
                    ContainerBuilder? builder = pair.builder;
                    builder.PreInitialize();
                }
            }
        }
        private void BuildComplete()
        {
            if (_containerFactories != null)
            {
                foreach ((ContainerFactory factory, ContainerBuilder builder) pair in _containerFactories)
                {
                    ContainerBuilder? builder = pair.builder;
                    builder.BuildComplete();
                }
            }
            
            InternalUtils.VerboseLog(Name, "Building Complete.", ContainerLogFlag.BuildingContainer);
            _container._onBuildComplete?.Invoke(_container);
        }
    }
}
#nullable disable