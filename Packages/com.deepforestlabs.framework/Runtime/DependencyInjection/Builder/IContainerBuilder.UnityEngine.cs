#nullable enable
using UnityEngine;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder
    {
        //TODO - [Obsolete("Doesn't do what we think it does.  See ActivationAnimationView")]
        IContainerBuilder AddScopedComponent<TComponent>()
            where TComponent : Component;
    }
}
#nullable disable