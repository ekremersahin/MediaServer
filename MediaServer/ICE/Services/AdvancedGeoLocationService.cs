using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class AdvancedGeoLocationService : IGeoLocationService
    {
        private readonly ILogger<AdvancedGeoLocationService> _logger;
        private readonly HttpClient _httpClient;

        public AdvancedGeoLocationService(
            ILogger<AdvancedGeoLocationService> logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<GeoLocation> GetCurrentLocationAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("https://ipapi.co/json/");
                return JsonSerializer.Deserialize<GeoLocation>(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mevcut konum tespit edilemedi");
                return GetDefaultLocation();
            }
        }

        public async Task<GeoLocation> GetCandidateLocationAsync(ICECandidate candidate)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://ipapi.co/{candidate.IpAddress}/json/");
                return JsonSerializer.Deserialize<GeoLocation>(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Aday {candidate.Id} için konum tespit edilemedi");
                return GetDefaultLocation();
            }
        }

        private GeoLocation GetDefaultLocation()
        {
            return new GeoLocation
            {
                Latitude = 0,
                Longitude = 0,
                CountryCode = "XX",
                City = "Unknown"
            };
        }

    }
}
