#nullable enable
using System;

namespace DeepForestLabs
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DependencyAttribute : Attribute
    {
    }
}
#nullable disable