#nullable enable
namespace DeepForestLabs.MVC.Models
{
    public enum ViewDownloadOptions
    {
        /// <summary>
        /// Downloaded during the container build.
        /// </summary>
        Required,
        
        /// <summary>
        /// Downloaded in background after container is built.
        /// </summary>
        Background,
        
        /// <summary>
        /// Downloaded when the view is needed.
        /// </summary>
        OnDemand
    }
}
#nullable disable