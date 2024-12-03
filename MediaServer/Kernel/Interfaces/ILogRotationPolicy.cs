using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface ILogRotationPolicy
    {
        Task CleanupLogsAsync(string logDirectory, TimeSpan maxLogAge, long maxTotalLogSizeMB);
        Task<bool> ShouldRotateLogsAsync(string logDirectory, long maxTotalLogSizeMB);
    }
}
