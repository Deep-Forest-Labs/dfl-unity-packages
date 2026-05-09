#nullable enable
using JetBrains.Annotations;
using UnityEngine;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
        [HideInCallstack]
        public static void DevError(string message)
        {
            DevError((Object?)null, message);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1>(string format, T1 arg1)
        {
            DevError((Object?)null, format, arg1);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            DevError((Object?)null, format, arg1, arg2);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            DevError((Object?)null, format, arg1, arg2, arg3);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            DevError((Object?)null, format, arg1, arg2, arg3, arg4);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            DevError(null, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [HideInCallstack]
        public static void DevError(Object? context, string message)
        {
#if RELEASE_BUILD
            Warning(context, message);
#else
            Error(context, message);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1>(Object? context, string format, T1 arg1)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1);
#else
            Error(context, format, arg1);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2>(Object? context, string format, T1 arg1, T2 arg2)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2);
#else
            Error(context, format, arg1, arg2);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2, T3>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2, arg3);
#else
            Error(context, format, arg1, arg2, arg3);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2, T3, T4>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2, arg3, arg4);
#else
            Error(context, format, arg1, arg2, arg3, arg4);
#endif
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevError<T1, T2, T3, T4, T5>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2, arg3, arg4, arg5);
#else
            Error(context, format, arg1, arg2, arg3, arg4, arg5);
#endif
        }
    }
}
#nullable disable