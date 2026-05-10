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
        private const string DEBUG_LOG_PREFIX =
#if !RELEASE_BUILD || RELEASE_WITH_DEBUG_LOGS && UNITY_EDITOR
        "<color=\"yellow\">DEBUG</color>: ";
#elif !RELEASE_BUILD || RELEASE_WITH_DEBUG_LOGS
		"DEBUG: ";
#else
        "";
#endif
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [HideInCallstack]
        public static void Debug(string message)
        {
            Debug((Object?)null, message);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1>(string format, T1 arg1)
        {
            Debug((Object?)null, format, arg1);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Debug((Object?)null, format, arg1, arg2);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Debug((Object?)null, format, arg1, arg2, arg3);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Debug((Object?)null, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Debug(null, format, arg1, arg2, arg3, arg4, arg5);
        }
        
         [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [HideInCallstack]
        public static void Debug(Object? context, string message)
        {
            message = ZString.Concat(GetCurrentFrameTag(), DEBUG_LOG_PREFIX, message);
            UnityEngine.Debug.Log(message, context);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1>(Object? context, string format, T1 arg1)
        {
            format = ZString.Concat(GetCurrentFrameTag(), DEBUG_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1), context);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2>(Object? context, string format, T1 arg1, T2 arg2)
        {
            format = ZString.Concat(GetCurrentFrameTag(), DEBUG_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2), context);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2, T3>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            format = ZString.Concat(GetCurrentFrameTag(), DEBUG_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3), context);
        }
		
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2, T3, T4>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            format = ZString.Concat(GetCurrentFrameTag(), DEBUG_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3, arg4), context);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Debug<T1, T2, T3, T4, T5>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            format = ZString.Concat(GetCurrentFrameTag(), DEBUG_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3, arg4, arg5), context);
        }
    }
}
#nullable disable