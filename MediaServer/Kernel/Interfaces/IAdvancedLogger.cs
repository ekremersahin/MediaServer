using MediaServer.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface IAdvancedLogger
    {
        void Log(LogLevel level, string message, Exception exception = null);
        void LogPerformance(string operation, long elapsedMilliseconds);
        void LogSecurityEvent(string eventType, string details);
        IDisposable BeginScope(string scopeName);
    }


}
