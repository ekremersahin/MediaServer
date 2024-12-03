using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using MediaServer.SignalizationServer.Interfaces;
using MediaServer.SignalizationServer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer.Services
{

    public class PeerDiscoveryService : IPeerDiscoveryService
    {
        private readonly ConcurrentDictionary<string, PeerMetadata> _peerMetadataStore =
            new ConcurrentDictionary<string, PeerMetadata>();

        private readonly ILogger<PeerDiscoveryService> _logger;
        private readonly ITelemetryService _telemetryService;

        public event Func<string, PeerStatus, Task> PeerStatusChanged;

        public PeerDiscoveryService(
            ILogger<PeerDiscoveryService> logger,
            ITelemetryService telemetryService)
        {
            _logger = logger;
            _telemetryService = telemetryService;
        }

        public async Task<IEnumerable<PeerMetadata>> GetAvailablePeersAsync(
            MediaType? mediaType = null,
            int? maxBandwidth = null)
        {
            var availablePeers = _peerMetadataStore.Values
                .Where(metadata =>
                    metadata.Status == PeerStatus.Online &&
                    (mediaType == null || metadata.SupportedMediaTypes.Contains(mediaType.Value)) &&
                    (maxBandwidth == null || metadata.Capabilities.MaxBandwidth >= maxBandwidth)
                )
                .ToList();

            return availablePeers;
        }

        public async Task UpdatePeerMetadataAsync(PeerMetadata metadata)
        {
            try
            {
                _peerMetadataStore.AddOrUpdate(
                    metadata.ClientId,
                    metadata,
                    (key, oldValue) =>
                    {
                        // Değişiklikleri kaydet
                        oldValue.Username = metadata.Username;
                        oldValue.SupportedMediaTypes = metadata.SupportedMediaTypes;
                        oldValue.Capabilities = metadata.Capabilities;
                        oldValue.Status = metadata.Status;
                        oldValue.LastActiveTimestamp = DateTime.UtcNow;
                        return oldValue;
                    }
                );

                // Status değişikliği varsa event tetikle
                await PeerStatusChanged?.Invoke(metadata.ClientId, metadata.Status);

                // Telemetri
                await _telemetryService.TrackMetricAsync(new TrackingModel
                {
                    MetricName = "PeerMetadataUpdated",
                    Value = 1,
                    Properties = new Dictionary<string, string>
                {
                    { "ClientId", metadata.ClientId },
                    { "Status", metadata.Status.ToString() }
                }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Peer metadata güncellemesinde hata");
                await _telemetryService.TrackErrorAsync(
                    new TrackingModel(),
                    new ErrorDetails
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                );
            }
        }

        public async Task<PeerMetadata> GetPeerMetadataAsync(string clientId)
        {
            return _peerMetadataStore.TryGetValue(clientId, out var metadata)
                ? metadata
                : null;
        }


    }
}
