#nullable enable
using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
        [HideInCallstack]
        public static void Exception(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        [HideInCallstack]
        public static void Exception(Exception exception, string message)
        { 
            Exception(null, exception, message);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1>(Exception exception, string format, T1 arg1)
        {
            Exception(null, exception, format, arg1);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2>(Exception exception, string format, T1 arg1, T2 arg2)
        {
            Exception(null, exception, format, arg1, arg2);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2, T3>(Exception exception, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Exception(null, exception, format, arg1, arg2, arg3);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2, T3, T4>(Exception exception, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4)
        {
            Exception(null, exception, format, arg1, arg2, arg3, arg4);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2, T3, T4, T5>(Exception exception, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4, T5 arg5)
        {
            Exception(null, exception, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [HideInCallstack]
        public static void Exception(Object? context, Exception exception, string message)
        {
            Warning(context, message);
            UnityEngine.Debug.LogException(exception);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1>(Object? context, Exception exception, string format, T1 arg1)
        {
            Warning(context, format, arg1);
            UnityEngine.Debug.LogException(exception);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2>(Object? context, Exception exception, string format, T1 arg1, T2 arg2)
        {
            Warning(context, format, arg1, arg2);
            UnityEngine.Debug.LogException(exception);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2, T3>(Object? context, Exception exception, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Warning(context, format, arg1, arg2, arg3);
            UnityEngine.Debug.LogException(exception);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2, T3, T4>(Object? context, Exception exception, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4)
        {
            Warning(context, format, arg1, arg2, arg3, arg4);
            UnityEngine.Debug.LogException(exception);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Exception<T1, T2, T3, T4, T5>(Object? context, Exception exception, string format, T1 arg1, T2 arg2, T3 arg3,
            T4 arg4, T5 arg5)
        {
            Warning(context, format, arg1, arg2, arg3, arg4, arg5);
            UnityEngine.Debug.LogException(exception);
        }
    }
}
#nullable disable