#nullable enable
using System;

namespace DeepForestLabs
{
    public interface IParentContainerBuilder
    {
    }

    public partial interface IContainerBuilder
    {
        IContainerBuilder AddSingleton(object singleton);

        IContainerBuilder AddAlias(Type alias, Type impl);
    }
}
#nullable disable
