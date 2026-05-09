#nullable enable
using System;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using JetBrains.Annotations;

namespace DeepForestLabs.Logger
{
    public sealed class GameException : Exception
    {
        public GameException(string? message) : base(message)
        {
        }

        public GameException(string? message, Exception innerException) : base(message, innerException)
        {
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1>(string format, T1 arg1)
        {
            return new GameException(ZString.Format(format, arg1));
        }

        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            return new GameException(ZString.Format(format, arg1, arg2));
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3));
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3, arg4));
        }

        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5));
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6));
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1>(Exception inner, string format, T1 arg1)
        {
            return new GameException(ZString.Format(format, arg1), inner);
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2>(Exception inner, string format, T1 arg1, T2 arg2)
        {
            return new GameException(ZString.Format(format, arg1, arg2), inner);
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3>(Exception inner, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3), inner);
        }
        
        [StringFormatMethod("format")]
        public static GameException FromFormat<T1, T2, T3, T4>(Exception inner, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return new GameException(ZString.Format(format, arg1, arg2, arg3, arg4), inner);
        }
    }
}
#nullable disable