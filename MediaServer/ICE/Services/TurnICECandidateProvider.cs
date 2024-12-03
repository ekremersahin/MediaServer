using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class TurnICECandidateProvider : IICECandidateProvider
    {
        private readonly ITurnClient _turnClient;
        private readonly ILogger<TurnICECandidateProvider> _logger;

        public TurnICECandidateProvider(
            ITurnClient turnClient,
            ILogger<TurnICECandidateProvider> logger)
        {
            _turnClient = turnClient;
            _logger = logger;
        }

        public async Task<IEnumerable<ICECandidate>> GetCandidatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var turnAllocations = await _turnClient.AllocateChannelsAsync();

                return turnAllocations.Select(allocation => new ICECandidate
                {
                    Type = "relay",
                    Protocol = "udp",
                    IpAddress = allocation.RelayedAddress,
                    Port = allocation.RelayedPort,
                    TransportType = "UDP",
                    Foundation = GenerateFoundation(allocation.RelayedAddress)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TURN sunucusundan aday alınamadı");
                return Enumerable.Empty<ICECandidate>();
            }
        }

        private string GenerateFoundation(string ipAddress)
            => $"turn-{ipAddress.GetHashCode()}";
    }

}
