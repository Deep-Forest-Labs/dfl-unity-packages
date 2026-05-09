#nullable enable
using System;

namespace DeepForestLabs
{
    public interface IMain
    {
        void Start();
        void PreRestart();
        void ShowingErrorPopup(Exception unhandled);
        void DismissingErrorPopup(Exception unhandled);
        void PostRestart();
    }
}
#nullable disable