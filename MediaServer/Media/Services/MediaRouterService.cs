using MediaServer.Media.Interfaces;
using MediaServer.RTC.Models;
using MediaServer.SDP.Interfaces;
using MediaServer.SignalizationServer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Services
{
    public class MediaRouterService : BaseMediaRouter
    {

        public MediaRouterService(ILogger<MediaRouterService> logger) : base(logger)
        {

        }

        public override async Task RegisterClientAsync(string clientId, MediaStream stream)
        {
            try
            {
                _activeStreams.TryAdd(clientId, stream);
                LogRouting($"Client {clientId} registered");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                throw;
            }
        }

        public override async Task RouteMediaAsync(string sourceClientId, MediaStream stream)
        {
            // Routing logic
            foreach (var clientId in _activeStreams.Keys.Where(k => k != sourceClientId))
            {
                // Broadcast veya hedefli routing
                //await _signalContext.SendMessageToClientAsync(clientId, stream);
            }
        }

        public override async Task UnregisterClientAsync(string clientId)
        {
            _activeStreams.TryRemove(clientId, out _);
            LogRouting($"Client {clientId} unregistered");
        }
    }
}
