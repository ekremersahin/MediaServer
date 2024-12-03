using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using MediaServer.ICE.Services;
using MediaServer.RTC.Interfaces;
using MediaServer.RTC.Models;
using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using MediaServer.SDP.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MediaServer.RTC.Services
{
    public class RTCPeerConnectionImpl : IRTCPeerConnection
    {
        private readonly ILogger<RTCPeerConnectionImpl> _logger;
        private readonly ICECandidateCollector _iceCandidateCollector;
        private readonly ICECandidateManager candidateManager;
        private readonly ISDPProcessor _sdpProcessor;
        private RTCPeerConnectionState _connectionState;

        public event EventHandler<MediaStream> OnTrack;
        public event EventHandler<RTCPeerConnectionState> OnConnectionStateChange;
        private SDPSessionDescription _localDescription;
        private SDPSessionDescription _remoteDescription;

        public RTCPeerConnectionState ConnectionState
        {
            get => _connectionState;
            private set
            {
                if (_connectionState != value)
                {
                    _connectionState = value;
                    OnConnectionStateChange?.Invoke(this, value);
                }
            }
        }


        public RTCPeerConnectionImpl(
                 ILogger<RTCPeerConnectionImpl> logger,
                 ICECandidateCollector iceCandidateCollector,
                 ICECandidateManager candidateManager,
                 ISDPProcessor sdpProcessor)
        {
            _logger = logger;
            _iceCandidateCollector = iceCandidateCollector;
            this.candidateManager = candidateManager;
            _sdpProcessor = sdpProcessor;
            _connectionState = RTCPeerConnectionState.New;
        }


        public async Task<string> CreateOfferAsync()
        {
            try
            {
                var candidates = await candidateManager.CollectAndPrioritizeCandidatesAsync();//_iceCandidateCollector.CollectCandidatesAsync();
                var sdpDescription = await _sdpProcessor.CreateOfferAsync(candidates);
                _localDescription = sdpDescription;// await _sdpProcessor.SetLocalDescriptionAsync(sdpDescription);
                ConnectionState = RTCPeerConnectionState.Connecting;
                return sdpDescription.Sdp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Offer oluşturulurken hata oluştu");
                ConnectionState = RTCPeerConnectionState.Failed;
                throw;
            }
        }
        public async Task<string> CreateAnswerAsync()
        {
            try
            {
                var candidates = await candidateManager.CollectAndPrioritizeCandidatesAsync();// _iceCandidateCollector.CollectCandidatesAsync();
                var sdpDescription = await _sdpProcessor.CreateAnswerAsync(candidates, _remoteDescription);
                _localDescription = sdpDescription;// await _sdpProcessor.SetLocalDescriptionAsync(sdpDescription);
                ConnectionState = RTCPeerConnectionState.Connecting;
                return sdpDescription.Sdp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Answer oluşturulurken hata oluştu");
                ConnectionState = RTCPeerConnectionState.Failed;
                throw;
            }
        }


        public async Task SetLocalDescriptionAsync(string sdp)
        {
            try
            {
                _localDescription = new SDPSessionDescription("local", sdp);
                //await _sdpProcessor.SetLocalDescriptionAsync(new SDPSessionDescription("local", sdp));
                ConnectionState = RTCPeerConnectionState.Connecting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local description ayarlanırken hata oluştu");
                ConnectionState = RTCPeerConnectionState.Failed;
                throw;
            }
        }

        public async Task SetRemoteDescriptionAsync(string sdp)
        {
            try
            {
                _remoteDescription = new SDPSessionDescription("remote", sdp);
                //await _sdpProcessor.SetRemoteDescriptionAsync(new SDPSessionDescription("remote", sdp));
                ConnectionState = RTCPeerConnectionState.Connecting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Remote description ayarlanırken hata oluştu");
                ConnectionState = RTCPeerConnectionState.Failed;
                throw;
            }
        }

        public Task AddIceCandidateAsync(string candidate)
        {
            // ICE adayını işle ve bağlantıyı güncelle

            if (_localDescription == null || _remoteDescription == null)
            {
                throw new InvalidOperationException("Local veya remote description henüz ayarlanmamış");
            }
            _sdpProcessor.AddIceCandidateAsync(ParseIceCandidate(candidate), _localDescription);

            return Task.CompletedTask;
        }

        private ICECandidate ParseIceCandidate(string candidate)
        {
            // Örnek bir parsing mekanizması
            // a=candidate:1 1 UDP 2122260223 192.168.1.100 54609 typ host
            var parts = candidate.Split(' ');

            return new ICECandidate
            {
                Foundation = parts[0].Split(':')[1],
                //Id = int.Parse(parts[1]),
                ComponentId = int.Parse(parts[1]),
                TransportType = parts[2],
                Priority = int.Parse(parts[3]),
                IpAddress = parts[4],
                Port = int.Parse(parts[5]),
                Type = parts[7],

            };
        }

        public Task Close()
        {
            ConnectionState = RTCPeerConnectionState.Closed;
            return Task.CompletedTask;
        }

        public Task AddTrackAsync(MediaStreamTrack track)
        {
            throw new NotImplementedException();
        }
    }
}