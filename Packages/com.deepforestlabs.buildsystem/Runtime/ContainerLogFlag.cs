namespace DeepForestLabs.BuildSystems
{
    [System.Flags]
    public enum ContainerLogFlag
    {
        CreatedContainer = 0x1 << 0,
        BuildingContainer = 0x1 << 1,
        Downloading = 0x1 << 2,
        Loading = 0x1 << 3,
        Instantiation = 0x1 << 4,
        Injection = 0x1 << 5,
        Initialization = 0x1 << 6,
        Running = 0x1 << 7,
        Disposing = 0x1 << 8,
        DisposingContainer = 0x1 << 9,
        Addressables = 0x1 << 10
    }
}
