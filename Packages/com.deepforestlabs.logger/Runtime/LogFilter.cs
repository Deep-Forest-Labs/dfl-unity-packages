#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs.Logger
{
    [CreateAssetMenu(menuName = "DFL/Log Filter", fileName = "LogFilter")]
    public class LogFilter : ScriptableObject, ILogFilter
    {
        public virtual bool IsIgnoredInfo(string? message)
        {
            return false;
        }

        public virtual bool IsIgnoredWarning(string? message)
        {
            return false;
        }

        public virtual bool IsIgnoredAssert(string? message)
        {
            return false;
        }

        public virtual bool IsIgnoredError(string? message)
        {
            return false;
        }

        public virtual bool IsIgnoredException(Exception? e)
        {
            return false;
        }

        public virtual bool IsWarningError(string formatted)
        {
            return false;
        }
    }
}
#nullable disable
