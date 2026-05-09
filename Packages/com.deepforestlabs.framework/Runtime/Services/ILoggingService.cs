#nullable enable
using System;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Services
{
    public interface ILoggingService
    {
        void CaptureUnhandledException(Exception? e, Object? context = null);
    }
}
#nullable disable