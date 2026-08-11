#nullable enable
using System.Collections.Generic;
using DeepForestLabs.Logger;

namespace DeepForestLabs.Platform.Internal
{
    /// <summary>
    /// Dev-only once-per-call-site logging for null platform services.
    /// </summary>
    internal static class NullPlatformLog
    {
        private static readonly HashSet<string> Logged = new();

        public static void Once(string callSite, string format, params object[] args)
        {
            if (!Logged.Add(callSite))
            {
                return;
            }

            if (args.Length == 0)
            {
                Log.Debug("{0}: {1}", callSite, format);
                return;
            }

            Log.Debug("{0}: {1}", callSite, string.Format(format, args));
        }
    }
}
#nullable disable
