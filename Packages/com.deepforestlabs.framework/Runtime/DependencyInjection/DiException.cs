#nullable enable
using System;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using JetBrains.Annotations;

namespace DeepForestLabs
{
    public sealed class DiException : Exception
    {
        public DiException(string message, Exception? inner = null) : base(message, inner)
        {
        }
        
        [StringFormatMethod("format")]
        public static DiException FromFormat<T1>(string format, T1 arg1)
        {
            return new DiException(ZString.Format(format, arg1));
        }

        [StringFormatMethod("format")]
        public static DiException FromFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            return new DiException(ZString.Format(format, arg1, arg2));
        }
        
        [StringFormatMethod("format")]
        public static DiException FromFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return new DiException(ZString.Format(format, arg1, arg2, arg3));
        }
        
        [StringFormatMethod("format")]
        public static DiException FromFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return new DiException(ZString.Format(format, arg1, arg2, arg3, arg4));
        }
    }
}
#nullable disable