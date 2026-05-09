#nullable enable
using System;

namespace DeepForestLabs
{
    public class RefreshExpiredException : Exception
    {
        public RefreshExpiredException() { }
        public RefreshExpiredException(string message) : base(message) { }
    }
}
#nullable disable