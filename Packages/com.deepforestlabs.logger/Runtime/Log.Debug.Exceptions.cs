#nullable enable
#if !RELEASE_BUILD || RELEASE_WITH_DEBUG_LOGS
#define RELEASE_WITH_DEBUG_LOGS
#endif
using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [HideInCallstack]
        public static void DebugException(Exception e)
        {
            Exception(e);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [HideInCallstack]
        public static void DebugException(Exception e, string message)
        {
            DebugException(null, e, message);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1>(Exception e, string format, T1 arg1)
        {
            DebugException(null, e, format, arg1);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2>(Exception e, string format, T1 arg1, T2 arg2)
        {
            DebugException(null, e, format, arg1, arg2);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2, T3>(Exception e, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            DebugException(null, e, format, arg1, arg2, arg3);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2, T3, T4>(Exception e, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4)
        {
            Exception(e, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2, T3, T4, T5>(Exception e, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4, T5 arg5)
        {
            DebugException(null, e, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [HideInCallstack]
        public static void DebugException(Object? context, Exception e, string message)
        {
            Exception(context, e, message);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1>(Object? context, Exception e, string format, T1 arg1)
        {
            Exception(context, e, format, arg1);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2>(Object? context, Exception e, string format, T1 arg1, T2 arg2)
        {
            Exception(context, e, format, arg1, arg2);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2, T3>(Object? context, Exception e, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Exception(context, e, format, arg1, arg2, arg3);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2, T3, T4>(Object? context, Exception e, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4)
        {
            Exception(context, e, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_DEBUG_LOGS")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DebugException<T1, T2, T3, T4, T5>(Object? context, Exception e, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4, T5 arg5)
        {
            Exception(context, e, format, arg1, arg2, arg3, arg4, arg5);
        }
    }
}
#nullable disable