#nullable enable
using System;

namespace DeepForestLabs.MVC.Factory
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FactoryMenuItemAttribute : Attribute 
    {
        public string? Name { get; }
        public string? Path { get; }
        public string? Shortcut { get; }

        public FactoryMenuItemAttribute(string? name, string? path = null, string? shortcut = null)
        {
            Name = name;
            Path = path;
            Shortcut = shortcut ?? string.Empty;
        }
    }
}
#nullable disable