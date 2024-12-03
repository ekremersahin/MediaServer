using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface ILogFileService
    {
        Task WriteLogAsync<T>(string directory, T logEntry, DateTime timestamp);
        Task<IEnumerable<T>> ReadLogsAsync<T>(string directory, DateTime startTime, DateTime endTime);
    }
}
