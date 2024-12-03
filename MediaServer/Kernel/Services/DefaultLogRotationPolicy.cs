using MediaServer.Kernel.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class DefaultLogRotationPolicy : ILogRotationPolicy
    {
        private readonly ILogger<DefaultLogRotationPolicy> _logger;
        private readonly IFileService _fileService;

        public DefaultLogRotationPolicy(
            ILogger<DefaultLogRotationPolicy> logger,
            IFileService fileService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public async Task CleanupLogsAsync(string logDirectory, TimeSpan maxLogAge, long maxTotalLogSizeMB)
        {
            try
            {
                var now = DateTime.UtcNow;
                var files = await _fileService.GetFilesAsync(logDirectory, "*.log");
                var ow = await IsLogDirectoryOversizedAsync(logDirectory, maxTotalLogSizeMB);
                var filesToDelete = files.Where(f => (now - _fileService.GetLastWriteTimeUtc(f)).TotalDays > maxLogAge.TotalDays || ow).ToList();
                //var filesToDelete = files
                //    .Where(f =>(now - _fileService.GetLastWriteTimeUtc(f)).TotalDays > maxLogAge.TotalDays || await IsLogDirectoryOversizedAsync(logDirectory, maxTotalLogSizeMB))
                //    .ToList();

                // Minimum 1 log dosyası kalacak şekilde silme
                if (filesToDelete.Count > 1)
                {
                    var sortedFiles = filesToDelete
                        .OrderBy(f => _fileService.GetLastWriteTimeUtc(f))
                        .Take(filesToDelete.Count - 1)
                        .ToList();

                    foreach (var file in sortedFiles)
                    {
                        await _fileService.DeleteFileAsync(file);
                        _logger.LogInformation($"Deleted log file: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during log cleanup");
            }
        }

        public async Task<bool> ShouldRotateLogsAsync(string logDirectory, long maxTotalLogSizeMB)
        {
            return await IsLogDirectoryOversizedAsync(logDirectory, maxTotalLogSizeMB);
        }

        private async Task<bool> IsLogDirectoryOversizedAsync(string logDirectory, long maxTotalLogSizeMB)
        {
            var files = await _fileService.GetFilesAsync(logDirectory, "*.log");
            var totalSizeMB = files.Sum(f => _fileService.GetFileSizeMB(f));

            return totalSizeMB > maxTotalLogSizeMB;
        }
    }
}
