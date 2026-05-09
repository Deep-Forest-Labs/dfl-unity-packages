#nullable enable
namespace DeepForestLabs.Reflections
{
    internal readonly struct FieldInjectResult
    {
        public string? Error { get; }
        public object? Transient { get; }

        public FieldInjectResult(string? error, object? transient)
        {
            Error = error;
            Transient = transient;
        }

        public static FieldInjectResult FromSuccess(object? transient = null)
        {
            return new FieldInjectResult(null, transient);
        }
        
        public static FieldInjectResult FromError(string error)
        {
            return new FieldInjectResult(error, false);
        }
        
    }
}
#nullable disable