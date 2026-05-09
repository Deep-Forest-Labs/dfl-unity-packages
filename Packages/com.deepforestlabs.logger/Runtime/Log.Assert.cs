#nullable enable
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace DeepForestLabs.Logger
{
    public static partial class Log
    {
        private const string ASSERT_PREFIX = "[Assert Failure] ";
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [HideInCallstack]
        public static void Assert([DoesNotReturnIf(false)] bool condition, string message)
        {
            Assert(condition, (Object?)null, message);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1)
        {
            Assert(condition, (Object?)null, format, arg1);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1, T2 arg2)
        {
            Assert(condition, (Object?)null, format, arg1, arg2);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1, T2 arg2,
            T3 arg3)
        {
            Assert(condition, (Object?)null, format, arg1, arg2, arg3);
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4)
        {
            Assert(condition, (Object?)null, format, arg1, arg2, arg3, arg4);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Assert(condition, (Object?)null, format, arg1, arg2, arg3, arg4, arg5);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5, T6>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Assert(condition, (Object?)null, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5, T6, T7>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            Assert(condition, (Object?)null, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5, T6, T7, T8>([DoesNotReturnIf(false)] bool condition, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            Assert(condition, null, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [HideInCallstack]
        public static void Assert([DoesNotReturnIf(false)] bool condition, Object? context, string message)
        {
            UnityEngine.Debug.Assert(condition, ZString.Concat(ASSERT_PREFIX, message), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, message);
            }
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1));
            }
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1, T2 arg2)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1, arg2), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1, arg2));
            }
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1, T2 arg2,
            T3 arg3)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1, arg2, arg3), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1, arg2, arg3));
            }
        }

        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1, arg2, arg3, arg4), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1, arg2, arg3, arg4));
            }
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1, arg2, arg3, arg4, arg5), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1, arg2, arg3, arg4, arg5));
            }
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5, T6>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1, arg2, arg3, arg4, arg5, arg6), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6));
            }
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5, T6, T7>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1, arg2, arg3, arg4, arg5, arg6, arg7), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
            }
        }
        
        [Conditional("NOT_RELEASE_BUILD"), Conditional("RELEASE_WITH_ASSERTS_ENABLED")]
        [StringFormatMethod("format")]
        [HideInCallstack]
        public static void Assert<T1, T2, T3, T4, T5, T6, T7, T8>([DoesNotReturnIf(false)] bool condition, Object? context, string format, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            UnityEngine.Debug.Assert(condition, ZString.Format(ZString.Concat(ASSERT_PREFIX, format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), context);

            if (!condition)
            {
                throw new AssertionException(ASSERT_PREFIX, ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
            }
        }
    }
}
#nullable disable