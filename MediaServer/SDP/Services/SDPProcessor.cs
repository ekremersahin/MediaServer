
using MediaServer.ICE.Models;
using MediaServer.Media.Interfaces;
using MediaServer.Media.Models;
using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Services
{
    public class SDPProcessor : ISDPProcessor
    {
        private readonly ISDPValidator _validator;
        private readonly ILogger<SDPProcessor> _logger;
        private readonly IMediaHandler _mediaHandler;
        private readonly IConnectionManager _connectionManager;
        private readonly ISDPParser _SDPParser;
        private readonly ISDPGenerator _SDPGenerator;

        public SDPProcessor(
            ISDPValidator validator,
            ILogger<SDPProcessor> logger,
            IMediaHandler mediaHandler,
            IConnectionManager connectionManager,
            ISDPParser SDPParser,
            ISDPGenerator _SDPGenerator)
        {

            _validator = validator;
            _logger = logger;
            _mediaHandler = mediaHandler;
            _connectionManager = connectionManager;
            this._SDPParser = SDPParser;
            _SDPGenerator = _SDPGenerator;
        }

        public async Task<ProcessResult> ProcessSessionAsync(SessionDescription session)
        {
            var result = new ProcessResult
            {
                SessionId = session.Origin?.SessionId,
                Success = false
            };

            try
            {
                var validationResult = _validator.Validate(session);
                if (!validationResult.IsValid)
                {
                    result.Errors.AddRange(validationResult.Errors.Select(e => e.Message));
                    return result;
                }

                var initialized = await ValidateAndInitializeAsync(session);
                if (!initialized)
                {
                    result.Errors.Add("Session initialization failed");
                    return result;
                }

                var connectionStatus = await EstablishConnectionAsync(session.Connection);
                if (connectionStatus != ConnectionStatus.Connected)
                {
                    result.Errors.Add($"Connection failed: {connectionStatus}");
                    return result;
                }

                foreach (var media in session.Media)
                {
                    var mediaResult = await ProcessMediaAsync(media);
                    result.MediaResults.Add(mediaResult);
                }

                result.Success = result.MediaResults.All(m => m.Success);
                result.Metadata["ProcessedAt"] = DateTime.UtcNow.ToString("O");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SDP session");
                result.Errors.Add($"Processing error: {ex.Message}");
            }

            return result;
        }

        public async Task<MediaProcessResult> ProcessMediaAsync(MediaDescription media)
        {
            var result = new MediaProcessResult
            {
                MediaType = media.Type,
                Port = media.Port,
                Protocol = media.Protocol
            };

            try
            {
                result.Parameters = await _mediaHandler.ExtractParametersAsync(media);
                result.Codecs = await _mediaHandler.GetSupportedCodecsAsync(media);

                var initialized = await _mediaHandler.InitializeMediaAsync(media);
                if (!initialized)
                {
                    throw new MediaInitializationException($"Failed to initialize {media.Type} media");
                }

                if (media.Attributes != null)
                {
                    foreach (var attr in media.Attributes)
                    {
                        await ProcessMediaAttributeAsync(attr.Key, attr.Value, result);
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing media type: {media.Type}");
                result.Success = false;
            }

            return result;
        }

        public async Task<bool> ValidateAndInitializeAsync(SessionDescription session)
        {
            try
            {
                await _connectionManager.InitializeAsync(session.Origin);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session initialization failed");
                return false;
            }
        }

        public async Task<ConnectionStatus> EstablishConnectionAsync(ConnectionInfo connection)
        {
            try
            {
                return await _connectionManager.EstablishConnectionAsync(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection establishment failed");
                return ConnectionStatus.Failed;
            }
        }

        private async Task ProcessMediaAttributeAsync(string key, string value, MediaProcessResult result)
        {
            switch (key.ToLower())
            {
                case "rtpmap":
                    await ProcessRtpMapAsync(value, result);
                    break;
                case "fmtp":
                    await ProcessFmtpAsync(value, result);
                    break;
                case "crypto":
                    await ProcessCryptoAsync(value, result);
                    break;
                default:
                    result.Parameters[key] = value;
                    break;
            }
        }

        private async Task ProcessRtpMapAsync(string value, MediaProcessResult result)
        {
            var parts = value.Split(' ');
            if (parts.Length >= 2)
            {
                var codec = await _mediaHandler.ParseCodecAsync(parts[1]);
                if (codec != null)
                {
                    result.Codecs.Add(codec);
                }
            }
        }

        private async Task ProcessFmtpAsync(string value, MediaProcessResult result)
        {
            var parameters = await _mediaHandler.ParseFormatParametersAsync(value);
            foreach (var param in parameters)
            {
                result.Parameters[$"fmtp_{param.Key}"] = param.Value;
            }
        }

        private async Task ProcessCryptoAsync(string value, MediaProcessResult result)
        {
            var cryptoParams = await _mediaHandler.ParseCryptoParametersAsync(value);
            result.Parameters["crypto_suite"] = cryptoParams.Suite;
            result.Parameters["crypto_key"] = cryptoParams.Key;
        }

        //private SDPSessionDescription _remoteDescription;

        public async Task<SDPSessionDescription> CreateOfferAsync(IEnumerable<ICECandidate> candidates)
        {
            try
            {
                var sessionId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var sdpBuilder = new StringBuilder();

                // SDP başlangıç bilgileri
                sdpBuilder.AppendLine("v=0");
                sdpBuilder.AppendLine($"o=- {sessionId} 1 IN IP4 0.0.0.0");
                sdpBuilder.AppendLine("s=-");
                sdpBuilder.AppendLine("t=0 0");

                // ICE adaylarını ekle
                foreach (var candidate in candidates)
                {
                    sdpBuilder.AppendLine($"a=candidate:{candidate.Foundation} 1 {candidate.TransportType} " +
                        $"{candidate.Priority} {candidate.IpAddress} {candidate.Port} typ {candidate.Type}");
                }

                var offer = new SDPSessionDescription("offer", sdpBuilder.ToString());
                return offer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SDP offer oluşturulurken hata oluştu");
                throw;
            }
        }

        public Task<SDPSessionDescription> CreateAnswerAsync(IEnumerable<ICECandidate> candidates, SDPSessionDescription remoteDescription)
        {
            try
            {
                var sessionId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var sdpBuilder = new StringBuilder();

                // SDP başlangıç bilgileri
                sdpBuilder.AppendLine("v=0");
                sdpBuilder.AppendLine($"o=- {sessionId} 1 IN IP4 0.0.0.0");
                sdpBuilder.AppendLine("s=-");
                sdpBuilder.AppendLine("t=0 0");

                // Remote description varsa ona göre answer oluştur
                if (remoteDescription != null)
                {
                    // Remote description'dan media bilgilerini al
                    var remoteSession = _SDPParser.Parse(remoteDescription.Sdp);

                    // Media bilgilerini işle
                    foreach (var media in remoteSession.Media)
                    {
                        sdpBuilder.AppendLine($"m={media.Type} {media.Port} {media.Protocol} {string.Join(" ", media.FormatIds)}");
                    }
                }

                // ICE adaylarını ekle
                foreach (var candidate in candidates)
                {
                    sdpBuilder.AppendLine($"a=candidate:{candidate.Foundation} 1 {candidate.TransportType} " +
                        $"{candidate.Priority} {candidate.IpAddress} {candidate.Port} typ {candidate.Type}");
                }

                var answer = new SDPSessionDescription("answer", sdpBuilder.ToString());
                return Task.FromResult(answer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SDP answer oluşturulurken hata oluştu");
                throw;
            }
        }

        //public Task SetLocalDescriptionAsync(SDPSessionDescription description)
        //{
        //    //_localDescription = description ?? throw new ArgumentNullException(nameof(description));
        //    return Task.CompletedTask;
        //}

        //public Task SetRemoteDescriptionAsync(SDPSessionDescription description)
        //{
        //    //_remoteDescription = description ?? throw new ArgumentNullException(nameof(description));
        //    return Task.CompletedTask;
        //}

        public async Task<SDPSessionDescription> AddIceCandidateAsync(ICECandidate candidate, SDPSessionDescription localDescription)
        {

            // ICE aday yönetimi
            //if (_localDescription == null || _remoteDescription == null)
            //{
            //    throw new InvalidOperationException("Local veya remote description henüz ayarlanmamış");
            //}

            // Candidate'ı mevcut SDP'ye ekle
            var sessionDescription = _SDPParser.Parse(localDescription.Sdp);

            // Candidate'ı ilgili media description'a ekle
            foreach (var media in sessionDescription.Media)
            {
                if (media.Attributes == null)
                    media.Attributes = new Dictionary<string, string>();

                media.Attributes[$"candidate:{candidate.Foundation}"] =
                    $"{candidate.TransportType} {candidate.Priority} {candidate.IpAddress} {candidate.Port} typ {candidate.Type}";
            }

            // Güncellenmiş SDP'yi yeniden oluştur
            return new SDPSessionDescription(localDescription.Type, _SDPGenerator.Generate(sessionDescription));

        }
    }

}
