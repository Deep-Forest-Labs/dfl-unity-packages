#nullable enable
using System.Collections.Generic;
using DeepForestLabs;
using UnityEngine;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// No-op implementations of framework UI/error analytics helpers.
    /// </summary>
    public sealed class NullAnalyticsUiHelpers : IAnalyticsErrorHelper, IAnalyticsUIEventHelper
    {
        public void Log(string condition, string? stackTrace, LogType type) { }

        public void ClickedEvent(
            string str1,
            string? str2,
            string? str3,
            string? str4,
            string? str5,
            double? amount,
            Dictionary<string, object?>? extraData)
        {
        }

        public void ClickedCloseEvent(
            string str1,
            string? str2,
            string? str3,
            string? str4,
            string? str5,
            double? amount,
            Dictionary<string, object?>? extraData)
        {
        }
    }
}
#nullable disable
