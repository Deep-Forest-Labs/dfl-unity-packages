#nullable enable

using System;

namespace DeepForestLabs.Factories
{
    public abstract class FeatureContainerBuilderFactory<TGameState> : ContainerBuilderFactory<TGameState> 
        where TGameState : Enum
    {
    }
}
#nullable disable