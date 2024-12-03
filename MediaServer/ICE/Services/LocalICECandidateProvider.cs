using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class LocalICECandidateProvider : IICECandidateProvider
    {
        private readonly INetworkInterfaceService _networkService;

        public LocalICECandidateProvider(INetworkInterfaceService networkService)
        {
            _networkService = networkService;
        }

        public async Task<IEnumerable<ICECandidate>> GetCandidatesAsync(CancellationToken cancellationToken = default)
        {
            var localInterfaces = await _networkService.GetLocalNetworkInterfacesAsync();

            return localInterfaces.Select(iface => new ICECandidate
            {
                Type = "host",
                Protocol = "udp",
                IpAddress = iface.IpAddress,
                Port = GenerateRandomPort(),
                TransportType = "UDP",
                Foundation = GenerateFoundation(iface.IpAddress)
            }).ToList();
        }

        private int GenerateRandomPort() => new Random().Next(10000, 60000);

        private string GenerateFoundation(string ipAddress)
            => ipAddress.GetHashCode().ToString();
    }
}
