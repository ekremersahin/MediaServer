using MediaServer.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface ITelemetryService
    {
        // Generic error tracking
        Task TrackErrorAsync<T>(T model, ErrorDetails errorDetails)
            where T : TrackingModel;

        // Error istatistiklerini alma
        Task<ErrorStatistics> GetErrorStatisticsAsync(TimeSpan period);

        // Performans izleme
        Task TrackPerformanceAsync<T>(T model) where T : TrackingModel;

        // Genel metrik izleme
        Task TrackMetricAsync<T>(T model) where T : TrackingModel;

        // Detaylı istatistik alma
        Task<IEnumerable<TrackingModel>> GetMetricsAsync(
            string metricName,
            TimeSpan period,
            Dictionary<string, string> filters = null
        );
    }

    //public interface ITrackingModel
    //{
    //    string MetricName { get; init; }
    //    double Value { get; init; }
    //    DateTime Timestamp { get; init; }

    //    // Tüm özel özellikler ve detaylar bu sözlük içinde
    //    Dictionary<string, string> Properties { get; init; }
    //}

}
