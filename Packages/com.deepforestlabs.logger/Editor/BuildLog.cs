#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using JetBrains.Annotations;

namespace DeepForestLabs.Logger
{
    public static class BuildLog
    {
        private const string BUILD_LOG_PREFIX = ":: INFO: ::";
        private const string BUILD_WARNING_PREFIX = ":: WARNING:  ::";
        private const string BUILD_ERROR_PREFIX = ":: ERROR:  ::";
        
        [Conditional("UNITY_EDITOR")]
        public static void Exception(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
        
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(string message)
        {
            message = ZString.Concat(BUILD_LOG_PREFIX, message);
            UnityEngine.Debug.Log(message);
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Info<T1>(string format, T1 arg1)
        {
            format = ZString.Concat(BUILD_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Info<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            format = ZString.Concat(BUILD_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            format = ZString.Concat(BUILD_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Info<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            format = ZString.Concat(BUILD_LOG_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3, arg4));
        }
        
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string message)
        {
            message = ZString.Concat(BUILD_WARNING_PREFIX, message);
            UnityEngine.Debug.Log(message);
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Warning<T1>(string format, T1 arg1)
        {
            format = ZString.Concat(BUILD_WARNING_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Warning<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            format = ZString.Concat(BUILD_WARNING_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Warning<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            format = ZString.Concat(BUILD_WARNING_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Warning<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            format = ZString.Concat(BUILD_WARNING_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3, arg4));
        }
        
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string message)
        {
            message = ZString.Concat(BUILD_ERROR_PREFIX, message);
            UnityEngine.Debug.Log(message);
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Error<T1>(string format, T1 arg1)
        {
            format = ZString.Concat(BUILD_ERROR_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Error<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            format = ZString.Concat(BUILD_ERROR_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            format = ZString.Concat(BUILD_ERROR_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3));
        }
		
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Error<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            format = ZString.Concat(BUILD_ERROR_PREFIX, format);
            UnityEngine.Debug.Log(ZString.Format(format, arg1, arg2, arg3, arg4));
        }
    }
}
#nullable disable
