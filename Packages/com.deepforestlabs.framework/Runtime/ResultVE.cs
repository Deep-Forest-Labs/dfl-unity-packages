#nullable enable
using System;
using System.Runtime.CompilerServices;
using DeepForestLabs.Logger;
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace DeepForestLabs
{
    public readonly struct ResultE
    {
        private readonly bool _isValid;
        private readonly string _errorMessage;

        public bool IsValid => _isValid;
        public string ErrorMessage => _errorMessage!;
        
        public void ThrowIsInvalid()
        {
            if (!IsValid)
            {
                throw GameException.FromFormat("Invalid result. {0}", _errorMessage);
            }
        }

        private ResultE(bool isValid, string errorMessage)
        {
            _isValid = isValid;
            _errorMessage = errorMessage ?? string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultE Success() => new(true, string.Empty);
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultE Error(string error) => new(false, error);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static ResultE Error<T1>(string format, T1 arg1) => new(false, ZString.Format(format, arg1));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static ResultE Error<T1, T2>(string format, T1 arg1, T2 arg2) => new(false, ZString.Format(format, arg1, arg2));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static ResultE Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3) => new(false, ZString.Format(format, arg1, arg2, arg3));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static ResultE Error<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4) => new(false, ZString.Format(format, arg1, arg2, arg3, arg4));
    }
    
    public readonly struct ResultE<E>
    {
        private readonly bool _isValid;
        private readonly E _error;

        public bool IsValid => _isValid;
        public E Error => _error;
        
        public void ThrowIsInvalid()
        {
            if (!IsValid)
            {
                throw GameException.FromFormat("Invalid result. {0}", _error);
            }
        }

        private ResultE(bool isValid, E error)
        {
            _isValid = isValid;
            _error = error;
        }

        public static ResultE<E> Success() => new(true, default!);
        public static ResultE<E> FromError(E error) => new(false, error);
    }
    
    public readonly struct ResultVE<V>
    {
        private readonly bool _isValid;
        private readonly V _value;
        private readonly string? _error;

        public bool IsValid => _isValid;
        
        public V Value
        {
            get
            {
                if (!_isValid)
                {
                    throw GameException.FromFormat("Invalid result. {0}", _error);
                }

                return _value;
            }
        }
        
        public string Error => _error ?? String.Empty;

        public void ThrowIsInvalid()
        {
            if (!IsValid)
            {
                throw GameException.FromFormat("Invalid result. {0}", _error);
            }
        }

        private ResultVE(bool isValid, V value, string? error)
        {
            _isValid = isValid;
            _value = value;
            _error = error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultVE<V> FromResult(V result) => new(true, result, null);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultVE<V> FromError(string error) => new(false, default!, error);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static ResultVE<V> FromError<T1>(string format, T1 arg1) => new(false, default!, ZString.Format(format, arg1));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static ResultVE<V> FromError<T1, T2>(string format, T1 arg1, T2 arg2) => new(false, default!, ZString.Format(format, arg1, arg2));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static ResultVE<V> FromError<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3) => new(false, default!, ZString.Format(format, arg1, arg2, arg3));
        
        public static implicit operator ResultE(ResultVE<V> result)
        {
            if (result.IsValid)
            {
                return ResultE.Success();
            }
            
            return ResultE.Error(result.Error);
        }
    }
    
    public readonly struct ResultVE<V, E>
    {
        private readonly bool _isValid;
        private readonly V _value;
        private readonly E _error;
        
        public bool IsValid => _isValid;

        public V Value
        {
            get
            {
                if (!_isValid)
                {
                    throw GameException.FromFormat("Invalid result. {0}", Error?.ToString());
                }

                return _value;
            }
        }

        public E Error => _error;
        
        public void ThrowIsInvalid()
        {
            if (!IsValid)
            {
                throw GameException.FromFormat("Invalid result. {0}", _error);
            }
        }

        internal ResultVE(bool isValid, V value, E error)
        {
            _isValid = isValid;
            _value = value;
            _error = error;
        }

        public static ResultVE<V, E> FromResult(V value) => new(true, value, default!);
        public static ResultVE<V, E> FromError(E error) => new(false, default!, error);
    }

        [Serializable]
    public struct VoidResult
    {
        [SerializeField] private bool _isValid;
        [SerializeField] private string _error;

        public bool IsValid => _isValid;
        public string Error => _error;
        
        public VoidResult(string? error)
        {
            _isValid = string.IsNullOrEmpty(error);
            _error = error!;
        }
        
        public static VoidResult FromSuccess()
        {
            return new VoidResult(null);
        }
        
        public static VoidResult FromError(string error) => new(error);
        
        [StringFormatMethod("format")]
        public static VoidResult FromError<T1>(string format, T1 arg1) => FromError(ZString.Format(format, arg1));
        
        [StringFormatMethod("format")]
        public static VoidResult FromError<T1, T2>(string format, T1 arg1, T2 arg2) => FromError(ZString.Format(format, arg1, arg2));
        
        [StringFormatMethod("format")]
        public static VoidResult FromError<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3) => FromError(ZString.Format(format, arg1, arg2, arg3));
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VoidResult FromError<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 args) => FromError(ZString.Format(format, arg1, arg2, arg3, args));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VoidResult FromError<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => FromError(ZString.Format(format, arg1, arg2, arg3, arg4, arg5));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VoidResult FromError(Exception error) => new(error.Message);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AssertIsValid()
        {
            Log.Assert(IsValid, Error);
        }
        
        public static implicit operator VoidResult(ResultV<VoidReturn> result)
        {
            if (result.IsValid)
            {
                return FromSuccess();
            }
            
            return FromError(result.Error);
        }
        
        public static implicit operator ResultV<VoidReturn>(VoidResult result)
        {
            if (result.IsValid)
            {
                return ResultV<VoidReturn>.FromResult(VoidReturn.Return);
            }
            
            return ResultV<VoidReturn>.FromError(result.Error);
        }
    }
    
    [Serializable]
    public struct ResultV<V>
    {
        [SerializeField] private bool _isValid;
        [SerializeField] private string _error;
        [SerializeField] private V _value;

        public bool IsValid => _isValid;
        public V Value
        {
            get
            {
                if (!_isValid)
                {
                    throw new GameException(_error);
                }

                return _value;
            }
        }
        public string Error => _error;
        
        public ResultV(V value)
        {
            _isValid = true;
            _value = value;
            _error = string.Empty;
        }
        
        public ResultV(string error)
        {
            _isValid = false;
            _value = default!;
            _error = error;
        }
        
        public static implicit operator ResultE(ResultV<V> result)
        {
            if (result.IsValid)
            {
                return ResultE.Success();
            }

            return ResultE.Error(result.Error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultV<V> FromResult(V value) => new(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultV<T> FromResult<T>(T value) => new(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultV<V> FromError(string error) => new(error);
        
        [StringFormatMethod("format")]
        public static ResultV<V> FromError<T1>(string format, T1 arg1) => FromError(ZString.Format(format, arg1));
        
        [StringFormatMethod("format")]
        public static ResultV<V> FromError<T1, T2>(string format, T1 arg1, T2 arg2) => FromError(ZString.Format(format, arg1, arg2));
        
        [StringFormatMethod("format")]
        public static ResultV<V> FromError<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3) => FromError(ZString.Format(format, arg1, arg2, arg3));
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultV<V> FromError<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 args) => FromError(ZString.Format(format, arg1, arg2, arg3, args));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultV<V> FromError<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => FromError(ZString.Format(format, arg1, arg2, arg3, arg4, arg5));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResultV<V> FromError(Exception error) => new(error.Message);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AssertIsValid()
        {
            Log.Assert(IsValid, Error);
        }
    }
}
#nullable disable