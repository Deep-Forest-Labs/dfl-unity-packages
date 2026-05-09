#nullable enable
using System.Collections;
using System.Diagnostics;
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
        private const string ASSET_VALIDATION_PREFIX = "[Asset Validation Failure] ";
        
        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [HideInCallstack]
        public static void Validate(Object? context, bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, ZString.Concat(GetCurrentFrameTag(), ASSET_VALIDATION_PREFIX, message), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1>(Object? context, bool condition, string format, T1 arg1)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(GetCurrentFrameTag(), ASSET_VALIDATION_PREFIX, format), arg1), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2>(Object? context, bool condition, string format, T1 arg1, T2 arg2)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(GetCurrentFrameTag(), ASSET_VALIDATION_PREFIX, format), arg1, arg2), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2, T3>(Object? context, bool condition, string format, T1 arg1, T2 arg2,
            T3 arg3)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(GetCurrentFrameTag(), ASSET_VALIDATION_PREFIX, format), arg1, arg2, arg3), context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2, T3, T4>(Object? context, bool condition, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(GetCurrentFrameTag(), ASSET_VALIDATION_PREFIX, format), arg1, arg2, arg3, arg4), context);
        }
        
        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2, T3, T4, T5>(Object? context, bool condition, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(GetCurrentFrameTag(), ASSET_VALIDATION_PREFIX, format), arg1, arg2, arg3, arg4, arg5), context);
        }
        
        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [HideInCallstack]
        public static void Validate(Object? context, IList componentList, string message)
        {
            foreach (object? element in componentList)
            {
                Validate(context, element != null, message);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1>(Object? context, IList componentList, string format, T1 arg1)
        {
            foreach (object? element in componentList)
            {
                Validate(context, element != null, format, arg1);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2>(Object? context, IList componentList, string format, T1 arg1, T2 arg2)
        {
            foreach (object? element in componentList)
            {
                Validate(context, element != null, format, arg1, arg2);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2, T3>(Object? context, IList componentList, string format, T1 arg1, T2 arg2,
            T3 arg3)
        {
            foreach (object? element in componentList)
            {
                Validate(context, element != null, format, arg1, arg2, arg3);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2, T3, T4>(Object? context, IList componentList, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4)
        {
            foreach (object? element in componentList)
            {
                Validate(context, element != null, format, arg1, arg2, arg3, arg4);
            }
        }
        
        [Conditional("UNITY_EDITOR"), Conditional("NOT_RELEASE_BUILD")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Validate<T1, T2, T3, T4, T5>(Object? context, IList componentList, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            foreach (object? element in componentList)
            {
                Validate(context, element != null, format, arg1, arg2, arg3, arg4, arg5);
            }
        }
    }
}
#nullable disable