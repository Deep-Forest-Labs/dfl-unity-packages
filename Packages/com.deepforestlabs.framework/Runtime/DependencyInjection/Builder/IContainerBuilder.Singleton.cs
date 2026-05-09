#nullable enable

using System;

namespace DeepForestLabs
{
    public interface IParentContainerBuilder
    {
        IContainerBuilder PromoteFrom<T>(IContainerBuilder child);
        IContainerBuilder Promote<T>(T instance);

    }
    public partial interface IContainerBuilder
    {
        IContainerBuilder AddSingleton<T>(T instance);
        IContainerBuilder AddSingleton(object singleton);

        IContainerBuilder AddAlias<A, T>()
            where T : A;

        IContainerBuilder AddAlias(Type alias, Type impl);
    }
}
#nullable disable