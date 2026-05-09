#nullable enable
using System;

namespace DeepForestLabs.MVC.Models
{
    [Flags]
    public enum SkippableControlStates
    {
        /// <summary>
        /// Cancels <see cref="IView.OpenAnimation"/> and <see cref="Controller<>.Open"/>. 
        /// </summary>
        Opening = 0x1 << 0,
        
        /// <summary>
        /// Cancels <see cref="Controller<>.Run(view, token) "/>. 
        /// </summary>
        Running = 0x1 << 1,
        
        /// <summary>
        /// Cancels <see cref="IView.CloseAnimation"/> and <see cref="Controller<>.Close"/>. 
        /// </summary>
        Closing = 0x1 << 2,
        
        /// <summary>
        /// Will skip PreClose/Close/PostClose before returning results.  PreClose/Close/PostClose still get called,
        /// but in background task while letting Run return.
        /// </summary>
        Return = 0x1 << 4, 
    }
}
#nullable disable