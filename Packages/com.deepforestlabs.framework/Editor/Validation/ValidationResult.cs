#nullable enable
using System;

namespace DeepForestLabs.EditorTools.Validation
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public sealed class ValidationResult
    {
        public ValidationSeverity Severity { get; }
        public string Message { get; }
        public string Category { get; }
        public Action? Fix { get; }

        public ValidationResult(ValidationSeverity severity, string message, string category, Action? fix = null)
        {
            Severity = severity;
            Message = message;
            Category = category;
            Fix = fix;
        }
    }
}
#nullable disable
