using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OptionalAttribute : PropertyAttribute
    {
        public bool ElementCanBeNull { get; } = false;

        public OptionalAttribute()
        {
        }

        public OptionalAttribute(bool elementCanBeNull)
        {
            ElementCanBeNull = elementCanBeNull;
        }
    }
}