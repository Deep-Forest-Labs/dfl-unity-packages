#nullable enable
using System.Collections.Generic;
using DeepForestLabs.Data;
using UnityEngine;

namespace DeepForestLabs.Factories
{
    public class ContainerFactory : ValidatedData
    {
        public virtual string Name => GetType().Name.Replace("Factory", "").Replace("Container", "");

        public virtual IContainerBuilder Resolve(IContainerBuilder parent, IContainerBuilder builder)
        {
            return builder;
        }

        public virtual void Arrange(IParentContainerBuilder parent, IContainerBuilder builder,
            IReadOnlyList<IContainer> children)
        {
        }
    }
}
#nullable disable