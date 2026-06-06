#nullable enable
using System;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder
    {
        IContainerBuilder AddScoped(Type t, Func<IDiCollection, object> factory);
    }
}
#nullable disable
