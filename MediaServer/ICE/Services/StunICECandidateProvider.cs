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
    public class StunICECandidateProvider : IICECandidateProvider
    {
        private readonly IStunClient _stunClient;
        private readonly ILogger<StunICECandidateProvider> _logger;

        public StunICECandidateProvider(
            IStunClient stunClient,
            ILogger<StunICECandidateProvider> logger)
        {
            _stunClient = stunClient;
            _logger = logger;
        }

        public async Task<IEnumerable<ICECandidate>> GetCandidatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var stunResponse = await _stunClient.GetPublicAddressAsync();

                return new[]
                {
                new ICECandidate
                {
                    Type = "srflx",
                    Protocol = "udp",
                    IpAddress = stunResponse.PublicIpAddress,
                    Port = stunResponse.PublicPort,
                    TransportType = "UDP",
                    Foundation = GenerateFoundation(stunResponse.PublicIpAddress)
                }
            };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "STUN sunucusundan aday alınamadı");
                return Enumerable.Empty<ICECandidate>();
            }
        }

        private string GenerateFoundation(string ipAddress)
            => $"stun-{ipAddress.GetHashCode()}";
    }
}
