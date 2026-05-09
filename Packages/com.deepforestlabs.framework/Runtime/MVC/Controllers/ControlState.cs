#nullable enable
namespace DeepForestLabs.MVC.Controllers
{
    public enum ControlState
    {
        Disabled,
        Enabled,
        PreOpen,
        Opening,
        PostOpen,
        Running,
        PreClose,
        Closing,
        PostClose
    }
}
#nullable disable