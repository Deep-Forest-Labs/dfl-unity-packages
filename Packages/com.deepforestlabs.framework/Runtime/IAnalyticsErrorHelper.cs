#nullable enable
using UnityEngine;

namespace DeepForestLabs
{
    public interface IAnalyticsErrorHelper
    {
        void Log(string condition, string? stackTrace, LogType type);
    }
}
#nullable disable