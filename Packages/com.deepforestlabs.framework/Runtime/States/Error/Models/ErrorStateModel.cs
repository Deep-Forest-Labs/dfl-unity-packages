#nullable enable
using System;

namespace DeepForestLabs.States.Error.Models
{
    public interface IErrorStateModel
    {
        Exception? Exception { get; set; }
    }
}
#nullable disable