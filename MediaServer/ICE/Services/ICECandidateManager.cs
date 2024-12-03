using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class ICECandidateOptions
    {
        public int MaxCandidates { get; set; } = 10;
        public TimeSpan CandidateLifetime { get; set; } = TimeSpan.FromMinutes(15);
    }

    public enum CandidateStatus
    {
        Discovered,
        Refreshed,
        Connected, Failed
    }

    public class CandidateState
    {
        public CandidateStatus Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CandidateCriteria
    {
        public string Type { get; set; }
        public string Protocol { get; set; }
        public long MinBandwidth { get; set; }
    }


    public class ICECandidateManager
    {
        private readonly CandidatePrioritizationManager _prioritizationManager;
        private readonly ILogger<ICECandidateManager> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly IOptions<ICECandidateOptions> _options;

        // Aday toplama sağlayıcıları
        private readonly IEnumerable<IICECandidateProvider> _candidateProviders;

        // Aday havuzu ve durumu
        private ConcurrentDictionary<string, ICECandidate> _candidatePool = new();
        private ConcurrentDictionary<string, CandidateState> _candidateStates = new();

        public ICECandidateManager(
            CandidatePrioritizationManager prioritizationManager,
            ILogger<ICECandidateManager> logger,
            ITelemetryService telemetryService,
            IOptions<ICECandidateOptions> options,
            IEnumerable<IICECandidateProvider> candidateProviders)
        {
            _prioritizationManager = prioritizationManager;
            _logger = logger;
            _telemetryService = telemetryService;
            _options = options;
            _candidateProviders = candidateProviders;
        }

        // Aday toplama ve önceliklendirme ana metodu
        public async Task<IEnumerable<ICECandidate>> CollectAndPrioritizeCandidatesAsync(
            PrioritizationStrategy strategy = PrioritizationStrategy.BalancedPerformance,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Tüm sağlayıcılardan adayları topla
                var candidateTasks = _candidateProviders
                    .Select(provider => provider.GetCandidatesAsync(cancellationToken))
                    .ToList();

                var candidateResults = await Task.WhenAll(candidateTasks);
                var allCandidates = candidateResults.SelectMany(c => c).ToList();

                // Adayları önceliklendirme
                var prioritizedCandidates = await _prioritizationManager
                    .PrioritizeCandidatesAsync(allCandidates, strategy);

                // Aday havuzunu güncelle
                UpdateCandidatePool(prioritizedCandidates);

                return prioritizedCandidates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aday toplama ve önceliklendirme sırasında hata");
                await _telemetryService.TrackErrorAsync(new TrackingModel
                {
                    MetricName = "CandidateCollectionError",
                    Properties = new Dictionary<string, string>
                {
                    { "ErrorMessage", ex.Message }
                }
                }, null);
                throw;
            }
        }

        // Aday havuzunu güncelle
        private void UpdateCandidatePool(IEnumerable<ICECandidate> candidates)
        {
            foreach (var candidate in candidates)
            {
                // Benzersiz aday kimliği oluştur
                var candidateKey = GenerateCandidateKey(candidate);

                // Adayı havuza ekle veya güncelle
                _candidatePool.AddOrUpdate(
                    candidateKey,
                    candidate,
                    (key, oldValue) => candidate
                );

                // Aday durumunu başlat
                _candidateStates.AddOrUpdate(
                    candidateKey,
                    new CandidateState
                    {
                        Status = CandidateStatus.Discovered,
                        LastUpdated = DateTime.UtcNow
                    },
                    (key, oldValue) =>
                    {
                        oldValue.Status = CandidateStatus.Refreshed;
                        oldValue.LastUpdated = DateTime.UtcNow;
                        return oldValue;
                    }
                );
            }
        }

        // Aday yönetimi metotları
        public async Task<ICECandidate> SelectBestCandidateAsync(
            CandidateCriteria criteria = null,
            CancellationToken cancellationToken = default)
        {
            // Kriterlere göre en iyi adayı seç
            var candidates = _candidatePool.Values.ToList();

            // Özel kriterler varsa filtrele
            if (criteria != null)
            {
                candidates = FilterCandidates(candidates, criteria);
            }

            // Boş aday listesi kontrolü
            if (!candidates.Any())
            {
                await CollectAndPrioritizeCandidatesAsync(cancellationToken: cancellationToken);
                candidates = _candidatePool.Values.ToList();
            }

            // En iyi adayı seç
            var bestCandidate = candidates
                .OrderByDescending(c => GetCandidatePriority(c))
                .FirstOrDefault();

            if (bestCandidate != null)
            {
                await _telemetryService.TrackPerformanceAsync(new TrackingModel
                {
                    MetricName = "BestCandidateSelected",

                    Properties = new Dictionary<string, string>
                {
                    { "Type", bestCandidate.Type },
                    { "Protocol", bestCandidate.Protocol },
                        { "Value" ,bestCandidate.Id }
                }
                });
            }

            return bestCandidate;
        }

        // Aday filtreleme metodu
        private List<ICECandidate> FilterCandidates(
            List<ICECandidate> candidates,
            CandidateCriteria criteria)
        {
            return candidates.Where(c =>
                (criteria.Type == null || c.Type == criteria.Type) &&
                (criteria.Protocol == null || c.Protocol == criteria.Protocol) &&
                (criteria.MinBandwidth == 0 || GetCandidateBandwidth(c) >= criteria.MinBandwidth)
            ).ToList();
        }

        // Aday öncelik hesaplama
        private double GetCandidatePriority(ICECandidate candidate)
        {
            // Aday durumuna göre öncelik hesaplama
            var state = _candidateStates.TryGetValue(GenerateCandidateKey(candidate), out var candidateState)
            ? candidateState
                : null;

            // Varsayılan öncelik hesaplaması
            return state?.Status switch
            {
                CandidateStatus.Connected => 100.0,
                CandidateStatus.Discovered => 75.0,
                CandidateStatus.Failed => 25.0,
                _ => 50.0
            };
        }

        // Aday bant genişliği alma
        private long GetCandidateBandwidth(ICECandidate candidate)
        {
            // Gerçek bant genişliği bilgisi için ek mekanizma gerekebilir
            return 10_000_000; // Varsayılan 10 Mbps
        }

        // Benzersiz aday anahtarı oluşturma
        private string GenerateCandidateKey(ICECandidate candidate)
        {
            return $"{candidate.IpAddress}:{candidate.Port}:{candidate.Type}";
        }
    }

}
