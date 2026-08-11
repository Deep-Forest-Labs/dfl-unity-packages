#nullable enable
using System.Collections.Generic;
using DeepForestLabs;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Forwards Controller click analytics into Firebase via <see cref="IAnalyticsService"/>.
    /// </summary>
    [Preserve]
    public sealed class FirebaseAnalyticsUiEventHelper : IAnalyticsUIEventHelper
    {
        [Dependency] private readonly IAnalyticsService _analytics = default!;

        public void ClickedEvent(
            string str1,
            string? str2,
            string? str3,
            string? str4,
            string? str5,
            double? amount,
            Dictionary<string, object?>? extraData)
        {
            TrackUiClick("click", str1, str2, str3, str4, str5, amount, extraData);
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
            TrackUiClick("close", str1, str2, str3, str4, str5, amount, extraData);
        }

        private void TrackUiClick(
            string kind,
            string str1,
            string? str2,
            string? str3,
            string? str4,
            string? str5,
            double? amount,
            Dictionary<string, object?>? extraData)
        {
            var parameters = new Dictionary<string, object?>
            {
                ["kind"] = kind,
                ["str1"] = str1
            };

            if (str2 != null) parameters["str2"] = str2;
            if (str3 != null) parameters["str3"] = str3;
            if (str4 != null) parameters["str4"] = str4;
            if (str5 != null) parameters["str5"] = str5;
            if (amount.HasValue) parameters["amount"] = amount.Value;

            if (extraData != null)
            {
                foreach (KeyValuePair<string, object?> pair in extraData)
                {
                    if (!string.IsNullOrEmpty(pair.Key) && !parameters.ContainsKey(pair.Key))
                    {
                        parameters[pair.Key] = pair.Value;
                    }
                }
            }

            _analytics.Track("ui_click", parameters);
        }
    }
}
#nullable disable
