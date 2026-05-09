#nullable enable
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
        [HideInCallstack]
        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), message));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1>(string format, T1 arg1)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1)));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1, arg2)));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1, arg2, arg3)));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1, arg2, arg3, arg4)));
        }
        
        [HideInCallstack]
        public static void Warning(Object? context, string message)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), message, context));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1>(Object? context, string format, T1 arg1)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1), context));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1, T2>(Object? context, string format, T1 arg1, T2 arg2)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1, arg2), context));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1, T2, T3>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1, arg2, arg3), context));
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1, T2, T3, T4>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1, arg2, arg3, arg4), context));
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Warning<T1, T2, T3, T4, T5>(Object? context, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            UnityEngine.Debug.LogWarning(ZString.Concat(GetCurrentFrameTag(), ZString.Format(format, arg1, arg2, arg3, arg4, arg5), context));
        }
    }
}
#nullable disable