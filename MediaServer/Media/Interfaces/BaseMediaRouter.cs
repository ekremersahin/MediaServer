using MediaServer.RTC.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Interfaces
{
    public abstract class BaseMediaRouter : MediaServer.Media.Interfaces.IMediaRouter
    {
        protected readonly ILogger<BaseMediaRouter> _logger;
        protected ConcurrentDictionary<string, MediaStream> _activeStreams;

        protected BaseMediaRouter(ILogger<BaseMediaRouter> logger)
        {
            _logger = logger;
            _activeStreams = new ConcurrentDictionary<string, MediaStream>();
        }

        public abstract Task RegisterClientAsync(string clientId, MediaStream stream);
        public abstract Task RouteMediaAsync(string sourceClientId, MediaStream stream);
        public abstract Task UnregisterClientAsync(string clientId);

        // Ortak yardımcı metodlar
        protected virtual void LogRouting(string message)
        {
            _logger.LogInformation($"MediaRouter: {message}");
        }
    }
}
