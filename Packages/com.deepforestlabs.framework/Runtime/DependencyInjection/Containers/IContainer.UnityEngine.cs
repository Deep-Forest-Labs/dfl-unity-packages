#nullable enable
using System.Threading;
using UnityEngine;

namespace DeepForestLabs
{
    public partial interface IContainer
    {
        IContainerBuilder CreateComponent<TComponent>(CancellationToken token)
            where TComponent : Component;
    }
}
#nullable disable