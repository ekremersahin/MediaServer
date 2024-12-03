using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using MediaServer.Kernel.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class AdvancedCandidatePrioritizationService : ICandidatePrioritizationService
    {
        private readonly ILogger<AdvancedCandidatePrioritizationService> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly INetworkConditionService _networkConditionService;
        private readonly IGeoLocationService _geoLocationService;
        private readonly CandidatePrioritizationOptions _options;

        public AdvancedCandidatePrioritizationService(
            ILogger<AdvancedCandidatePrioritizationService> logger,
            ITelemetryService telemetryService,
            INetworkConditionService networkConditionService,
            IGeoLocationService geoLocationService,
            IOptions<CandidatePrioritizationOptions> options)
        {
            _logger = logger;
            _telemetryService = telemetryService;
            _networkConditionService = networkConditionService;
            _geoLocationService = geoLocationService;
            _options = options.Value;
        }

        public async Task<IEnumerable<ICECandidate>> PrioritizeCandidatesAsync(
            IEnumerable<ICECandidate> candidates,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var networkConditions = await _networkConditionService.GetCurrentConditionAsync(cancellationToken);
                var currentLocation = await _geoLocationService.GetCurrentLocationAsync();

                var prioritizedCandidates = await Task.WhenAll(
                    candidates.Select(async candidate =>
                        await EvaluateCandidateAsync(candidate, networkConditions, currentLocation, cancellationToken)
                    )
                );

                return prioritizedCandidates
                    .OrderByDescending(x => x.Priority)
                    .Select(x => x.Candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Candidate prioritization failed");
                return candidates; // Hata durumunda orijinal listeyi döndür
            }
        }

        private async Task<(ICECandidate Candidate, double Priority)> EvaluateCandidateAsync(
            ICECandidate candidate,
            NetworkCondition networkConditions,
            GeoLocation currentLocation,
            CancellationToken cancellationToken)
        {
            double basePriority = CalculateBasePriority(candidate);
            double networkScore = await CalculateNetworkScoreAsync(candidate, networkConditions, cancellationToken);
            double locationScore = CalculateGeographicalScore(candidate, currentLocation);

            double finalPriority = CalculateFinalPriority(
                basePriority,
                networkScore,
                locationScore
            );

            await LogCandidatePriorityTelemetryAsync(candidate, finalPriority);

            return (candidate, finalPriority);
        }

        private double CalculateBasePriority(ICECandidate candidate)
        {
            return candidate.CandidateTypeEnum switch
            {
                CandidateType.Host => _options.BaseHostPriority,
                CandidateType.ServerReflexive => _options.BaseServerReflexivePriority,
                CandidateType.PeerReflexive => _options.BasePeerReflexivePriority,
                CandidateType.Relayed => _options.BaseRelayedPriority,
                _ => 0
            };
        }

        private async Task<double> CalculateNetworkScoreAsync(
            ICECandidate candidate,
            NetworkCondition networkConditions,
            CancellationToken cancellationToken)
        {
            double latencyScore = CalculateLatencyScore(networkConditions.Latency);
            double bandwidthScore = CalculateBandwidthScore(networkConditions.Bandwidth);
            double packetLossScore = CalculatePacketLossScore(networkConditions.PacketLoss);

            // Opsiyonel: Candidate bazlı ek network testleri



            //var candidateNetworkTest = await TestCandidateNetworkPerformanceAsync(candidate, cancellationToken);

            return (latencyScore * _options.LatencyWeightFactor) +
                   (bandwidthScore * _options.BandwidthWeightFactor) +
                   (packetLossScore * _options.PacketLossWeightFactor);
            // (candidateNetworkTest.Score ?? 0);
        }

        private double CalculateLatencyScore(double latency)
        {
            return latency switch
            {
                < 50 => 100,   // Mükemmel
                < 100 => 75,   // İyi
                < 200 => 50,   // Orta
                _ => 25         // Zayıf
            };
        }

        private double CalculateBandwidthScore(long bandwidth)
        {
            return bandwidth switch
            {
                > 100_000_000 => 100,  // Fiber/Gigabit
                > 50_000_000 => 75,    // Yüksek hız
                > 10_000_000 => 50,    // Orta hız
                _ => 25                // Düşük hız
            };
        }

        private double CalculatePacketLossScore(double packetLoss)
        {
            return packetLoss switch
            {
                < 0.1 => 100,  // Mükemmel
                < 1 => 75,     // İyi
                < 3 => 50,     // Orta
                _ => 25         // Zayıf
            };
        }

        private double CalculateGeographicalScore(
            ICECandidate candidate,
            GeoLocation currentLocation)
        {
            var candidateLocation = _geoLocationService.GetCandidateLocationAsync(candidate).Result;
            double distance = CalculateHaversineDistance(
                currentLocation.Latitude,
                currentLocation.Longitude,
                candidateLocation.Latitude,
                candidateLocation.Longitude
            );

            return distance switch
            {
                < 50 => 100,   // Çok yakın
                < 200 => 75,   // Yakın
                < 500 => 50,   // Orta mesafe
                _ => 25         // Uzak
            };
        }

        private double CalculateFinalPriority(
        double basePriority,
        double networkScore,
            double locationScore)
        {
            return basePriority +
                   (networkScore * _options.LatencyWeightFactor) +
                   (locationScore * _options.GeographicalProximityWeightFactor);
        }

        private async Task LogCandidatePriorityTelemetryAsync(ICECandidate candidate, double finalPriority)
        {
            await _telemetryService.TrackPerformanceAsync(new TrackingModel
            {
                MetricName = candidate.Id,
                Value = finalPriority
            });
        }

        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Dünya'nın yarıçapı (km)
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Mesafe (km)
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
