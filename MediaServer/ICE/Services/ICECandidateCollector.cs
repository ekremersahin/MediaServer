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
    public class ICECandidateCollector
    {
        private readonly IEnumerable<IICECandidateProvider> _providers;
        private readonly ILogger<ICECandidateCollector> _logger;

        public ICECandidateCollector(
            IEnumerable<IICECandidateProvider> providers,
            ILogger<ICECandidateCollector> logger)
        {
            _providers = providers;
            _logger = logger;
        }

        public async Task<IEnumerable<ICECandidate>> CollectCandidatesAsync(CancellationToken cancellationToken = default)
        {
            var candidates = new List<ICECandidate>();

            foreach (var provider in _providers)
            {
                try
                {
                    var providerCandidates = await provider.GetCandidatesAsync(cancellationToken);
                    candidates.AddRange(providerCandidates);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Aday toplama hatası: {provider.GetType().Name}");
                }
            }

            return candidates;
        }
    }

}
