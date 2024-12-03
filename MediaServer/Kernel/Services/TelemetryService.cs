using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly ILogger<TelemetryService> _logger;
        private readonly ILogFileService _logFileService;
        private readonly string _errorLogDirectory;
        private readonly string _metricsLogDirectory;
        private readonly string _performanceLogDirectory;
        private readonly LogRotationOptions _rotationOptions;
        private readonly ILogRotationPolicy _logRotationPolicy;
        public TelemetryService(ILogger<TelemetryService> logger, ILogFileService logFileService, ILogRotationPolicy logRotationPolicy, IOptions<LogRotationOptions> rotationOptions)
        {
            _rotationOptions = rotationOptions.Value;
            _logRotationPolicy = logRotationPolicy;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logFileService = logFileService ?? throw new ArgumentNullException(nameof(logFileService));


            _errorLogDirectory = Path.Combine(_rotationOptions.LogDirectory, "Errors");
            _metricsLogDirectory = Path.Combine(_rotationOptions.LogDirectory, "Metrics");
            _performanceLogDirectory = Path.Combine(_rotationOptions.LogDirectory, "Performance");

            var di1 = Directory.CreateDirectory(_errorLogDirectory);
            var di2 = Directory.CreateDirectory(_metricsLogDirectory);
            var di3 = Directory.CreateDirectory(_performanceLogDirectory);
        }

        public async Task<ErrorStatistics> GetErrorStatisticsAsync(TimeSpan period)
        {
            var now = DateTime.UtcNow;
            var startTime = now - period;

            var filteredErrors = await _logFileService.ReadLogsAsync<ErrorDetails>(_errorLogDirectory, startTime, now);

            var errorStatistics = new ErrorStatistics
            {
                TotalErrorCount = filteredErrors.Count(),
                ErrorsBySeverity = filteredErrors.GroupBy(e => e.Severity)
                                                 .ToDictionary(g => g.Key, g => g.Count()),
                ErrorsByType = filteredErrors.GroupBy(e => e.ExceptionType)
                                             .ToDictionary(g => g.Key, g => g.Count()),
                StartTime = startTime,
                EndTime = now
            };

            return errorStatistics;
        }

        public async Task<IEnumerable<TrackingModel>> GetMetricsAsync(string metricName, TimeSpan period, Dictionary<string, string> filters = null)
        {
            var now = DateTime.UtcNow;
            var startTime = now - period;

            var filteredMetrics = await _logFileService.ReadLogsAsync<TrackingModel>(_metricsLogDirectory, startTime, now);

            filteredMetrics = filteredMetrics.Where(m => m.MetricName == metricName);

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    filteredMetrics = filteredMetrics.Where(m => m.Properties.ContainsKey(filter.Key) && m.Properties[filter.Key] == filter.Value);
                }
            }

            return filteredMetrics;
        }

        public async Task TrackErrorAsync<T>(T model, ErrorDetails errorDetails) where T : TrackingModel
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (errorDetails == null)
                errorDetails = new ErrorDetails() { Category = ErrorCategory.Unknown, Message = model.MetricName, AdditionalContext = model.Properties, Environment = Environment.OSVersion.Platform.ToString(), MachineName = Environment.MachineName, Timestamp = model.Timestamp };
            //throw new ArgumentNullException(nameof(errorDetails));

            // Log the error details
            _logger.LogError("Error tracked: {ErrorId}, {Timestamp}, {ExceptionType}, {Message}, {Severity}, {Category}, {MachineName}, {ApplicationName}, {Environment}",
                errorDetails.ErrorId,
                errorDetails.Timestamp,
                errorDetails.ExceptionType,
                errorDetails.Message,
                errorDetails.Severity,
                errorDetails.Category,
                errorDetails.MachineName,
                errorDetails.ApplicationName,
                errorDetails.Environment);

            // Write the error details to a file
            await WriteLogWithRotationAsync(_errorLogDirectory, errorDetails, errorDetails.Timestamp);
        }

        public async Task TrackMetricAsync<T>(T model) where T : TrackingModel
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Log the metric details
            _logger.LogInformation("Metric tracked: {MetricName}, {Value}, {Timestamp}, {Properties}",
                model.MetricName,
                model.Value,
                model.Timestamp,
                model.Properties);


            // Write the metric details to a file
            await WriteLogWithRotationAsync(_metricsLogDirectory, model, model.Timestamp);
        }

        public async Task TrackPerformanceAsync<T>(T model) where T : TrackingModel
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Log the performance details
            _logger.LogInformation("Performance tracked: {MetricName}, {Value}, {Timestamp}, {Properties}",
                model.MetricName,
                model.Value,
                model.Timestamp,
                model.Properties);

            // Write the performance details to a file
            await WriteLogWithRotationAsync(_performanceLogDirectory, model, model.Timestamp);
        }

        private async Task WriteLogWithRotationAsync<T>(string directory, T model, DateTime timestamp)
        {
            await PerformLogRotationAsync(directory);
            await _logFileService.WriteLogAsync(directory, model, timestamp);
        }

        private async Task PerformLogRotationAsync(string logDirectory)
        {
            try
            {
                // Log rotasyonu kontrol et
                if (await _logRotationPolicy.ShouldRotateLogsAsync(
                    logDirectory,
                    _rotationOptions.MaxTotalLogSizeMB))
                {
                    await _logRotationPolicy.CleanupLogsAsync(
                        logDirectory,
                        _rotationOptions.MaxLogAge,
                        _rotationOptions.MaxTotalLogSizeMB);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log rotation failed");
            }
        }

    }
}