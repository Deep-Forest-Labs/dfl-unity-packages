#nullable enable
namespace DeepForestLabs
{
    public interface IManagedGameObjectCallbackReceiver
    {
        void OnCheckedOut();
        void OnCheckedIn();
    }
}
#nullable disable