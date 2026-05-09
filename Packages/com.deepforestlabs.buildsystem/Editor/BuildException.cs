#nullable enable
using System;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using JetBrains.Annotations;

namespace DeepForestLabs.BuildSystems
{
    public sealed class BuildException : Exception
    {
        public BuildException(string? message) : base(message)
        {
        }

        public BuildException(string? message, Exception innerException) : base(message, innerException)
        {
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1>(string format, T1 arg1)
        {
            return new BuildException(ZString.Format(format, arg1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            return new BuildException(ZString.Format(format, arg1, arg2));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3, arg4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1>(Exception inner, string format, T1 arg1)
        {
            return new BuildException(ZString.Format(format, arg1), inner);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2>(Exception inner, string format, T1 arg1, T2 arg2)
        {
            return new BuildException(ZString.Format(format, arg1, arg2), inner);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3>(Exception inner, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3), inner);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static BuildException FromFormat<T1, T2, T3, T4>(Exception inner, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return new BuildException(ZString.Format(format, arg1, arg2, arg3, arg4), inner);
        }
    }
}
#nullable disable