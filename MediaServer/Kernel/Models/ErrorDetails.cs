using MediaServer.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Models
{
    public record ErrorDetails
    {
        public Guid ErrorId { get; init; } = Guid.NewGuid();
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string ExceptionType { get; init; }
        public string Message { get; init; }
        public string StackTrace { get; init; }
        public ErrorSeverity Severity { get; init; }
        public ErrorCategory Category { get; init; }
        public RecoveryStrategy RecommendedStrategy { get; init; }
        public string MachineName { get; init; } = System.Environment.MachineName;
        public string ApplicationName { get; init; }
        public string Environment { get; init; }
        public Dictionary<string, string> AdditionalContext { get; init; } = new();
    }
}
