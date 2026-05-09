#nullable enable
using DeepForestLabs.Factories;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder
    {
        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/>, loads the <see cref="ContainerBuilderFactory"/> at <paramref name="assetRef"/>
        /// and run <see cref="ContainerBuilderFactory.AddToBuilder"/> 
        /// </summary>
        IContainerBuilder AddFromBuilder<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ContainerBuilderFactory;

        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/> run <see cref="ContainerBuilderFactory.AddToBuilder"/> of <paramref name="factory"/> 
        /// </summary>
        IContainerBuilder AddFromBuilder(ContainerBuilderFactory factory);
        
        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/> run <see cref="ContainerBuilderFactory.AddToBuilder"/> of <paramref name="factory"/>
        /// pass <paramref name="arg"/> 
        /// </summary>
        IContainerBuilder AddFromBuilder<TArg>(ContainerBuilderFactory<TArg> factory, TArg arg);

        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/> run <see cref="ContainerBuilderFactory.AddToBuilder"/> of <paramref name="factory"/>
        /// pass <paramref name="arg1"/> and <paramref name="arg2"/> 
        /// </summary>
        IContainerBuilder AddFromBuilder<TArg1, TArg2>(ContainerBuilderFactory<TArg1, TArg2> factory, TArg1 arg1,
            TArg2 arg2);

        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/>, loads the <see cref="ContainerFactory"/> at <paramref name="assetRef"/>
        /// then add its the current builder as a child.
        /// </summary>
        IContainerBuilder AddChild<T>(ScriptableObjectAssetRefT<T> assetRef) where T : ContainerFactory;
        
        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/>, loads the <see cref="ContainerFactory"/> at <paramref name="key"/>
        /// then add its the current builder as a child.
        /// </summary>
        IContainerBuilder AddChild(ContainerFactory factory);
    }
}
#nullable disable