using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class CentralErrorManager : IErrorHandler
    {
        private readonly IErrorClassifier _classifier;
        private readonly IErrorLogger _logger;
        private readonly ITelemetryService _telemetry;
        private readonly IErrorNotificationService _notificationService;
        private readonly ILogger<CentralErrorManager> _systemLogger;

        public CentralErrorManager(
            IErrorClassifier classifier,
            IErrorLogger logger,
            ITelemetryService telemetry,
            IErrorNotificationService notificationService,
            ILogger<CentralErrorManager> systemLogger)
        {
            _classifier = classifier;
            _logger = logger;
            _telemetry = telemetry;
            _notificationService = notificationService;
            _systemLogger = systemLogger;
        }

        public async Task<RecoveryStrategy> HandleErrorAsync(
           Exception ex,
           RecoveryStrategy defaultStrategy = RecoveryStrategy.LogAndContinue,
           [CallerMemberName] string callerMethod = "",
           [CallerFilePath] string callerFile = "")
        {

            var errorDetails = new ErrorDetails
            {
                ExceptionType = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                Severity = _classifier.ClassifySeverity(ex),
                Category = _classifier.ClassifyCategory(ex),
                ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name,
                RecommendedStrategy = _classifier.DetermineRecoveryStrategy(ex) != default ? _classifier.DetermineRecoveryStrategy(ex) : defaultStrategy,
                Environment = Environment.MachineName,
                AdditionalContext = new Dictionary<string, string>
            {
                { "CallerMethod", callerMethod },
                { "CallerFile", callerFile }
            }
            };

            // Paralel işlemler
            await Task.WhenAll(
                _logger.LogErrorAsync(errorDetails),
                _telemetry.TrackErrorAsync<TrackingModel>(new TrackingModel()
                {
                    MetricName = "",
                    Properties = new Dictionary<string, string>() {
                    { "Message",ex.Message },
                    { "StackTrace",ex.StackTrace }
                },
                    Timestamp = DateTime.Now,
                    Value = 0
                }, errorDetails),
                HandleCriticalErrors(errorDetails)
            );

            return errorDetails.RecommendedStrategy;
        }


        private async Task HandleCriticalErrors(ErrorDetails errorDetails)
        {
            if (errorDetails.Severity >= ErrorSeverity.High)
            {
                await _notificationService.NotifyAsync(errorDetails);
                _systemLogger.LogCritical(
                    "High severity error detected: {ErrorId}",
                    errorDetails.ErrorId
                );
            }
        }

    }

}
