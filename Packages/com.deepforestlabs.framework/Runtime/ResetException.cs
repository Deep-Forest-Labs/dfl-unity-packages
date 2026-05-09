#nullable enable
using System;

namespace DeepForestLabs
{
    /// <summary>
    /// Special exception that is caught during IGameLayer.Run and treated as an intentional reset of the game.
    /// Player is NOT logged out
    /// No error dialog or sentry event are captured.
    /// </summary>
    internal class ResetException : Exception
    {
        public bool ClearLoginData { get; }
        
        public ResetException(bool clearLoginData, string? message) : base(message)
        {
            ClearLoginData = clearLoginData;
        }
    }
}
#nullable disable