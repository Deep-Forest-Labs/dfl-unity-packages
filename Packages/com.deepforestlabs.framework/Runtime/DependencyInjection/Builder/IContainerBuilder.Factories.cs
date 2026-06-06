#nullable enable
using DeepForestLabs.Factories;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder
    {
        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/> run <see cref="ContainerBuilderFactory.AddToBuilder"/> of <paramref name="factory"/>
        /// </summary>
        IContainerBuilder AddFromBuilder(ContainerBuilderFactory factory);

        /// <summary>
        /// On <see cref="IContainerBuilder.Build"/>, loads the <see cref="ContainerFactory"/> at <paramref name="factory"/>
        /// then add its the current builder as a child.
        /// </summary>
        IContainerBuilder AddChild(ContainerFactory factory);
    }
}
#nullable disable
