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
        public static void DevException(Exception e)
        {
#if RELEASE_BUILD
			Warning(e.Message);
#else
            Exception(e);
#endif
        }
        
        [HideInCallstack]
        public static void DevException(Exception e, string message)
        {
            DevException(null, e, message);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1>(Exception e, string format, T1 arg1)
        {
            DevException(null, e, format, arg1);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2>(Exception e, string format, T1 arg1, T2 arg2)
        {
            DevException(null, e, format, arg1, arg2);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2, T3>(Exception e, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            DevException(null, e, format, arg1, arg2, arg3);
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2, T3, T4>(Exception e, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            DevException(null, e, format, arg1, arg2, arg3, arg4);
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2, T3, T4, T5>(Exception e, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            DevException(null, e, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [HideInCallstack]
        public static void DevException(Object? context, Exception e, string message)
        {
#if RELEASE_BUILD
			Warning(context, message);
#else
            Exception(context, e, message);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1>(Object? context, Exception e, string format, T1 arg1)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1);
#else
            Exception(context, e, format, arg1);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2>(Object? context, Exception e, string format, T1 arg1, T2 arg2)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2);
#else
            Exception(context, e, format, arg1, arg2);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2, T3>(Object? context, Exception e, string format, T1 arg1, T2 arg2, T3 arg3)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2, arg3);
#else
            Exception(context, e, format, arg1, arg2, arg3);
#endif
        }
        
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2, T3, T4>(Object? context, Exception e, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2, arg3, arg4);
#else
            Exception(context, e, format, arg1, arg2, arg3, arg4);
#endif
        }

        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void DevException<T1, T2, T3, T4, T5>(Object? context, Exception e, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
#if RELEASE_BUILD
			Warning(context, format, arg1, arg2, arg3, arg4);
#else
            Exception(context, e, format, arg1, arg2, arg3, arg4);
#endif
        }
    }
}
#nullable disable