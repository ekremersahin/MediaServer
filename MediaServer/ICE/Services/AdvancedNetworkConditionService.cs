using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class AdvancedNetworkConditionService : INetworkConditionService
    {
        private readonly ILogger<AdvancedNetworkConditionService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _performanceCache;
        private readonly NetworkPerformanceOptions _options;
        private readonly SemaphoreSlim _testSemaphore;

        public AdvancedNetworkConditionService(
            ILogger<AdvancedNetworkConditionService> logger,
            HttpClient httpClient,
            IMemoryCache memoryCache,
            IOptions<NetworkPerformanceOptions> options)
        {
            _logger = logger;
            _httpClient = httpClient;
            _performanceCache = memoryCache;
            _options = options.Value;
            _testSemaphore = new SemaphoreSlim(_options.MaxConcurrentTests);
        }

        public async Task<NetworkCondition> GetCurrentConditionAsync(CancellationToken cancellationToken = default)
        {
            // Önbellekten kontrol
            if (_performanceCache.TryGetValue("NetworkCondition", out NetworkCondition cachedCondition))
            {
                return cachedCondition;
            }

            try
            {
                var tasks = new List<Task<double>>
                {
                    MeasureLatencyAsync(),
                    MeasureBandwidthAsync(),
                    MeasurePacketLossAsync()
                };

                await Task.WhenAll(tasks);

                var networkCondition = new NetworkCondition
                {
                    Latency = await tasks[0],
                    Bandwidth = (long)await tasks[1],
                    PacketLoss = await tasks[2]
                };

                // Önbelleğe alma
                _performanceCache.Set("NetworkCondition", networkCondition,
                    TimeSpan.FromMinutes(_options.CacheDurationMinutes));

                return networkCondition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ağ performansı ölçümünde hata");
                throw;
            }
        }

        public async Task<NetworkCondition> TestCandidateNetworkPerformanceAsync(
            ICECandidate candidate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _testSemaphore.WaitAsync(cancellationToken);

                var latencyTask = MeasureLatencyToTargetAsync(candidate.IpAddress);
                var bandwidthTask = MeasureBandwidthToTargetAsync(candidate.IpAddress);
                var packetLossTask = MeasurePacketLossToTargetAsync(candidate.IpAddress);

                await Task.WhenAll(latencyTask, bandwidthTask, packetLossTask);

                return new NetworkCondition
                {
                    Latency = await latencyTask,
                    Bandwidth = await bandwidthTask,
                    PacketLoss = await packetLossTask
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Aday {candidate.Id} için ağ performans testi başarısız");
                throw;
            }
            finally
            {
                _testSemaphore.Release();
            }
        }

        private async Task<double> MeasureLatencyAsync(string targetHost = "8.8.8.8")
        {
            using var ping = new Ping();
            var result = await ping.SendPingAsync(targetHost, _options.PingTimeout);
            return result.RoundtripTime;
        }

        private async Task<double> MeasureLatencyToTargetAsync(string ipAddress)
        {
            using var ping = new Ping();
            var result = await ping.SendPingAsync(ipAddress, _options.PingTimeout);
            return result.RoundtripTime;
        }

        private async Task<double> MeasureBandwidthAsync()
        {
            var speedTestResults = new ConcurrentBag<long>();

            var tasks = _options.SpeedTestServers.Select(async server =>
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    var response = await _httpClient.GetAsync($"{server}?bytes=10000000");
                    var content = await response.Content.ReadAsByteArrayAsync();
                    stopwatch.Stop();

                    var bandwidth = (long)(content.Length * 8 / (stopwatch.ElapsedMilliseconds / 1000.0));
                    speedTestResults.Add(bandwidth);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Bant genişliği testi başarısız: {server}");
                }
            });

            await Task.WhenAll(tasks);

            return speedTestResults.Any()
                ? speedTestResults.Average()
                : 10_000_000; // Varsayılan 10 Mbps
        }

        private async Task<long> MeasureBandwidthToTargetAsync(string ipAddress)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://{ipAddress}/__speedtest");
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsByteArrayAsync();
                stopwatch.Stop();

                return (long)(content.Length * 8 / (stopwatch.ElapsedMilliseconds / 1000.0));
            }
            catch
            {
                return 5_000_000; // Varsayılan 5 Mbps
            }
        }

        private async Task<double> MeasurePacketLossAsync(string targetHost = "8.8.8.8")
        {
            int totalPings = 10;
            int failedPings = 0;

            for (int i = 0; i < totalPings; i++)
            {
                try
                {
                    using var ping = new Ping();
                    var result = await ping.SendPingAsync(targetHost, _options.PingTimeout);
                    if (result.Status != IPStatus.Success)
                        failedPings++;
                }
                catch
                {
                    failedPings++;
                }
            }

            return (double)failedPings / totalPings * 100;
        }

        private async Task<double> MeasurePacketLossToTargetAsync(string ipAddress)
        {
            int totalPings = 10;
            int failedPings = 0;

            for (int i = 0; i < totalPings; i++)
            {
                try
                {
                    using var ping = new Ping();
                    var result = await ping.SendPingAsync(ipAddress, _options.PingTimeout);
                    if (result.Status != IPStatus.Success)
                        failedPings++;
                }
                catch
                {
                    failedPings++;
                }
            }

            return (double)failedPings / totalPings * 100;
        }
    }

}
