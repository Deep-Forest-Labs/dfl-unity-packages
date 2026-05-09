#nullable enable
using System;
using System.Threading;
using UnityEngine;

namespace DeepForestLabs
{
    internal sealed partial class Container
    {
        public IContainerBuilder CreateComponent<TComponent>(CancellationToken token)
            where TComponent : Component
        {
            throw new NotImplementedException("//TODO [2.5.+] mwood implement as needed post 2.5.0");
        }
    }
}
#nullable disable