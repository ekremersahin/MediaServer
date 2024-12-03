using MediaServer.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class LogFileService : ILogFileService
    {
        public async Task WriteLogAsync<T>(string directory, T logEntry, DateTime timestamp)
        {
            var filePath = GetLogFilePath(directory, timestamp);
            var content = JsonSerializer.Serialize(logEntry);
            await File.AppendAllTextAsync(filePath, content + Environment.NewLine);
        }

        public async Task<IEnumerable<T>> ReadLogsAsync<T>(string directory, DateTime startTime, DateTime endTime)
        {
            var logFiles = GetLogFiles(directory, startTime, endTime);
            var logs = new List<T>();

            foreach (var file in logFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var logEntries = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(line => JsonSerializer.Deserialize<T>(line));
                logs.AddRange(logEntries);
            }

            return logs;
        }

        private string GetLogFilePath(string directory, DateTime timestamp)
        {
            var fileName = $"{timestamp:yyyyMMddHH}.log";
            return Path.Combine(directory, fileName);
        }

        private IEnumerable<string> GetLogFiles(string directory, DateTime startTime, DateTime endTime)
        {
            var startFile = $"{startTime:yyyyMMddHH}.log";
            var endFile = $"{endTime:yyyyMMddHH}.log";

            return Directory.GetFiles(directory, "*.log")
                            .Where(f => string.Compare(Path.GetFileName(f), startFile) >= 0 && string.Compare(Path.GetFileName(f), endFile) <= 0);
        }
    }
}
