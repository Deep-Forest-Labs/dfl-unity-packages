#nullable enable
namespace DeepForestLabs.MVC.Models
{
    public enum ViewLoadOptions
    {
        /// <summary>
        /// Loaded during the container build/create.  Will override <see cref="ViewDownloadOptions"/>
        /// </summary>
        Required,
        
        /// <summary>
        /// Loaded in background after container is built. Not applicable to create.  Will override <see cref="ViewDownloadOptions"/>
        /// </summary>
        Background,
        
        /// <summary>
        /// Loaded when view is needed.  Will unload when no longer being used.
        /// </summary>
        OnDemand,
        
        //TODO [2.5.+] implement something for this
        // /// <summary>
        // /// Will disable view functions if the asset fails to load.  Will log an Assert if that is the case.
        // /// 
        // /// Controller handles having no view by not calling any function that pass 'TView view'.  Instead skips them.
        // /// </summary>
        // Optional,
        //
        // /// <summary>
        // /// View is disabled and never loaded or instantiated.
        // ///
        // /// Controller handles having no view by not calling any function that pass 'TView view'.  Instead skips them. 
        // /// </summary>
        // Disabled,
    }
}
#nullable disable