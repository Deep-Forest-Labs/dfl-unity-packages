#nullable enable
#if !RELEASE_BUILD || RELEASE_WITH_DEBUG_LOGS
#define RELEASE_WITH_DEBUG_LOGS
#endif

using System.Diagnostics;
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
        private const string DEBUG_WARNING_PREFIX =
#if NOT_RELEASE_BUILD || RELEASE_WITH_DEBUG_LOGS && UNITY_EDITOR
        "<color=\"yellow\">DEBUG WARNING</color>: ";
#elif NOT_RELEASE_BUILD || RELEASE_WITH_DEBUG_LOGS
		"DEBUG WARNING: ";
#else
		"";
#endif
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [HideInCallstack]
        public static void DebugWarning(string message)
        {
            DebugWarning((Object?)null, message);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1>(string format, T1 arg1)
        {
            DebugWarning((Object?)null, format, arg1);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            DebugWarning((Object?)null, format, arg1, arg2);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            DebugWarning((Object?)null, format, arg1, arg2, arg3);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            DebugWarning((Object?)null, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            DebugWarning(null, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [HideInCallstack]
        public static void DebugWarning(Object? context, string message)
        {
            Warning(context, ZString.Concat(DEBUG_WARNING_PREFIX, message));
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1>(Object? context, string format, T1 arg1)
        {
            Warning(context, ZString.Concat(DEBUG_WARNING_PREFIX, format), arg1);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2>(Object? context, string format, T1 arg1, T2 arg2)
        {
            Warning(context, ZString.Concat(DEBUG_WARNING_PREFIX, format), arg1, arg2);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2, T3>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Warning(context, ZString.Concat(DEBUG_WARNING_PREFIX, format), arg1, arg2, arg3);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2, T3, T4>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Warning(context, ZString.Concat(DEBUG_WARNING_PREFIX, format), arg1, arg2, arg3, arg4);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugWarning<T1, T2, T3, T4, T5>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Warning(context, ZString.Concat(DEBUG_WARNING_PREFIX, format), arg1, arg2, arg3, arg4, arg5);
        }
    }
}
#nullable disable