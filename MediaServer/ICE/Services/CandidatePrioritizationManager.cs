using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public enum PrioritizationStrategy
    {
        LowestLatency,
        HighestBandwidth,
        BalancedPerformance,
        GeographicalProximity
    }

    public class CandidateEvaluation
    {
        public ICECandidate Candidate { get; set; }
        public double PerformanceScore { get; set; }
        public NetworkCondition NetworkCondition { get; set; }
        public GeoLocation Location { get; set; }
        public double GeographicalDistance { get; set; }
    }

    public class CandidatePrioritizationManager
    {
        private readonly INetworkConditionService _networkConditionService;
        private readonly IGeoLocationService _geoLocationService;
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<CandidatePrioritizationManager> _logger;

        public CandidatePrioritizationManager(
            INetworkConditionService networkConditionService,
            IGeoLocationService geoLocationService,
            ITelemetryService telemetryService,
            ILogger<CandidatePrioritizationManager> logger)
        {
            _networkConditionService = networkConditionService;
            _geoLocationService = geoLocationService;
            _telemetryService = telemetryService;
            _logger = logger;
        }

        public async Task<List<ICECandidate>> PrioritizeCandidatesAsync(
            List<ICECandidate> candidates,
            PrioritizationStrategy strategy = PrioritizationStrategy.BalancedPerformance)
        {
            var currentLocation = await _geoLocationService.GetCurrentLocationAsync();
            var currentNetworkCondition = await _networkConditionService.GetCurrentConditionAsync();

            var candidateEvaluations = new List<CandidateEvaluation>();

            foreach (var candidate in candidates)
            {
                var candidateEvaluation = await EvaluateCandidateAsync(candidate, currentLocation, currentNetworkCondition);
                candidateEvaluations.Add(candidateEvaluation);
            }

            // Stratejiye göre sıralama
            return PrioritizeCandidatesByStrategy(candidateEvaluations, strategy);
        }

        private async Task<CandidateEvaluation> EvaluateCandidateAsync(
            ICECandidate candidate,
            GeoLocation currentLocation,
            NetworkCondition currentNetworkCondition)
        {
            var candidateLocation = await _geoLocationService.GetCandidateLocationAsync(candidate);
            var candidateNetworkCondition = await _networkConditionService.TestCandidateNetworkPerformanceAsync(candidate);

            // Coğrafi mesafe hesaplama
            double distance = CalculateGeographicalDistance(currentLocation, candidateLocation);

            // Performans skorunu hesapla
            double performanceScore = CalculatePerformanceScore(
                currentNetworkCondition,
                candidateNetworkCondition,
                distance
            );

            // Telemetri kaydı
            await _telemetryService.TrackMetricAsync(new TrackingModel
            {
                MetricName = candidate.Id,
                Value = performanceScore,
                Properties = new Dictionary<string, string>
            {
                { "Latency", candidateNetworkCondition.Latency.ToString() },
                { "Bandwidth", candidateNetworkCondition.Bandwidth.ToString() },
                { "PacketLoss", candidateNetworkCondition.PacketLoss.ToString() },
                { "Distance", distance.ToString() }
            }
            });

            return new CandidateEvaluation
            {
                Candidate = candidate,
                PerformanceScore = performanceScore,
                NetworkCondition = candidateNetworkCondition,
                Location = candidateLocation,
                GeographicalDistance = distance
            };
        }

        private List<ICECandidate> PrioritizeCandidatesByStrategy(
            List<CandidateEvaluation> evaluations,
            PrioritizationStrategy strategy)
        {
            return strategy switch
            {
                PrioritizationStrategy.LowestLatency =>
                    evaluations.OrderBy(e => e.NetworkCondition.Latency)
                        .Select(e => e.Candidate)
                        .ToList(),

                PrioritizationStrategy.HighestBandwidth =>
                    evaluations.OrderByDescending(e => e.NetworkCondition.Bandwidth)
                        .Select(e => e.Candidate)
                        .ToList(),

                PrioritizationStrategy.BalancedPerformance =>
                    evaluations.OrderByDescending(e =>
                        CalculateBalancedScore(e.NetworkCondition, e.GeographicalDistance))
                        .Select(e => e.Candidate)
                        .ToList(),

                PrioritizationStrategy.GeographicalProximity =>
                    evaluations.OrderBy(e => e.GeographicalDistance)
                        .Select(e => e.Candidate)
                        .ToList(),

                _ => throw new ArgumentException("Geçersiz önceliklendirme stratejisi")
            };
        }

        private double CalculatePerformanceScore(
            NetworkCondition currentCondition,
            NetworkCondition candidateCondition,
            double distance)
        {
            // Performans skoru hesaplama algoritması
            double latencyScore = 1 / (1 + Math.Abs(currentCondition.Latency - candidateCondition.Latency));
            double bandwidthScore = candidateCondition.Bandwidth / (currentCondition.Bandwidth + 1);
            double packetLossScore = 1 / (1 + candidateCondition.PacketLoss);
            double distanceScore = 1 / (1 + distance);

            // Ağırlıklı hesaplama
            return (
                0.4 * latencyScore +
                0.3 * bandwidthScore +
                0.2 * packetLossScore +
                0.1 * distanceScore
            ) * 100;
        }

        private double CalculateBalancedScore(NetworkCondition condition, double distance)
        {
            // Dengeli bir performans skoru hesaplama
            return (
                (1 / (1 + condition.Latency)) * 0.4 +
                (condition.Bandwidth / 10_000_000) * 0.3 +
                (1 / (1 + condition.PacketLoss)) * 0.2 +
                (1 / (1 + distance)) * 0.1
            ) * 100;
        }

        private double CalculateGeographicalDistance(GeoLocation location1, GeoLocation location2)
        {
            // Haversine formülü ile coğrafi mesafe hesaplaması
            const double R = 6371; // Dünya yarıçapı (km)

            var lat1 = location1.Latitude * Math.PI / 180;
            var lon1 = location1.Longitude * Math.PI / 180;
            var lat2 = location2.Latitude * Math.PI / 180;
            var lon2 = location2.Longitude * Math.PI / 180;

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }

}
