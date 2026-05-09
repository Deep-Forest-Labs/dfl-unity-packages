#nullable enable
namespace DeepForestLabs.MVC.Constants
{
    internal static class MVCConstants
    {
#if !UNITY_EDITOR
        internal static readonly UnityEngine.Color COLOR = UnityEngine.Color.magenta;
        internal static readonly string COLOR_TAG_OPEN = "<color=\"" + COLOR + "\">";
        internal static readonly string COLOR_TAG_CLOSE = "<\\color>";
#else
        internal static readonly string COLOR_TAG_OPEN = string.Empty;
        internal static readonly string COLOR_TAG_CLOSE = string.Empty;
#endif
    }
}
#nullable disable