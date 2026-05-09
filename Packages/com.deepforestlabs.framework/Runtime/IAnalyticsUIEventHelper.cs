#nullable enable
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace DeepForestLabs
{
    [Preserve][RequireImplementors]
    public interface IAnalyticsUIEventHelper
    {
        void ClickedEvent(string str1, string? str2, string? str3, string? str4, string? str5, double? amount, Dictionary<string, object?>? extraData);
        void ClickedCloseEvent(string str1, string? str2, string? str3, string? str4, string? str5, double? amount, Dictionary<string, object?>? extraData);
    }
}
#nullable disable