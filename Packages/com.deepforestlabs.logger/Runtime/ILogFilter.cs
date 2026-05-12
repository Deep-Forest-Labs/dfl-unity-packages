#nullable enable
using System;

namespace DeepForestLabs.Logger
{
    public interface ILogFilter
    {
        bool IsIgnoredInfo(string? message);
        bool IsIgnoredWarning(string? message);
        bool IsIgnoredAssert(string? message);
        bool IsIgnoredError(string? message);
        bool IsIgnoredException(Exception? e);
        bool IsWarningError(string formatted);
    }
}
#nullable disable
