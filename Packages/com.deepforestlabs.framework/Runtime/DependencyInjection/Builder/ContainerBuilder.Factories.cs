#nullable enable
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Factories;
using DeepForestLabs.Logger;

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder
    {
        public IContainerBuilder AddFromBuilder<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ContainerBuilderFactory
        {
            async UniTask<object> Resolver(IDiCollection _, CancellationToken t)
            {
                return await _container.Checkout(assetRef, t);
            }

            _builderFactoryResolvers ??= new();
            _builderFactoryResolvers.Add(assetRef, Resolver);

            return AddScriptableObject(assetRef);
        }

        public IContainerBuilder AddFromBuilder(ContainerBuilderFactory factory)
        {
            Log.Assert(factory != null, "AddFromBuilder received a null ContainerBuilderFactory.");
            factory.AddToBuilder(this);
            return this;
        }

        public IContainerBuilder AddFromBuilder<TArg>(ContainerBuilderFactory<TArg> factory, TArg arg)
        {
            Log.Assert(factory != null, "AddFromBuilder<{0}> received a null ContainerBuilderFactory.", typeof(TArg).Name);
            factory.AddToBuilder(this, arg);
            return this;
        }

        public IContainerBuilder AddFromBuilder<TArg1, TArg2>(ContainerBuilderFactory<TArg1, TArg2> factory, TArg1 arg1,
            TArg2 arg2)
        {
            Log.Assert(factory != null, "AddFromBuilder<{0},{1}> received a null ContainerBuilderFactory.", typeof(TArg1).Name, typeof(TArg2).Name);
            factory.AddToBuilder(this, arg1, arg2);
            return this;
        }

        public IContainerBuilder AddChild<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ContainerFactory
        {
            _containerFactories ??= new();

            _containerFactoryResolvers ??= new();

            async UniTask<object> Resolver(IDiCollection _, CancellationToken t)
            {
                return await _container.Checkout(assetRef, t);
            }

            _containerFactoryResolvers.Add((_containerFactories.Count, assetRef), 
                Resolver);
            _containerFactories.Add((default!, default!));

            return AddDownload(assetRef);
        }

        public IContainerBuilder AddChild(ContainerFactory factory)
        {
            Log.Assert(factory != null, "AddChild received a null ContainerFactory.");
            _containerFactories ??= new();

            string name = ZString.Format("{0}[{1}]", factory.Name, _containerFactories.Count);
            ContainerBuilder builder = new(name, _container, _container._gameObject, true);
            _containerFactories.Add((factory, builder));

            return this;
        }
    }
}
#nullable disable