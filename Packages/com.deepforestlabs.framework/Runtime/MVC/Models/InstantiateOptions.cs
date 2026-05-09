#nullable enable
using System;

namespace DeepForestLabs.MVC.Models
{
    [Flags]
    public enum InstantiateOptions
    {
        /// <summary>
        /// Will Create/Destroy instances and not pool or cache
        /// </summary>
        Create,
        
        /// <summary>
        /// Will keep a cache of checked in instances for later use.
        /// </summary>
        Checkout,
    }
}
#nullable disable