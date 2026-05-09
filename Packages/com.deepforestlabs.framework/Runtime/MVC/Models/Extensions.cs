#nullable enable

namespace DeepForestLabs.MVC.Models
{
    public static class Extensions
    {
        /// <summary>
        /// ViewStates.Showing || ViewStates.Visible || ViewStates.Hiding;
        /// </summary>
        public static bool IsRendering(this ViewState viewState)
        {
            return viewState == ViewState.Showing ||
                   viewState == ViewState.Visible ||
                   viewState == ViewState.Hiding;
        }
    }
}
#nullable disable