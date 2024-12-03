using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using MediaServer.SDP.Services;
using MediaServer.SignalizationServer.Interfaces;
using MediaServer.SignalizationServer.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer.Services
{
    public class WebSocketMediator : IWebSocketMediator
    {
        private readonly IPeerDiscoveryService _peerDiscoveryService;
        private readonly ISDPHandler _sdpHandler;
        private readonly ILogger<WebSocketMediator> _logger;

        public WebSocketMediator(
            IPeerDiscoveryService peerDiscoveryService,
            ISDPHandler sdpHandler,
            ILogger<WebSocketMediator> logger)
        {
            _peerDiscoveryService = peerDiscoveryService;
            _sdpHandler = sdpHandler;
            _logger = logger;

            _messageHandlers.Add("offer");
            _messageHandlers.Add("answer");
            _messageHandlers.Add("candidate");
            _messageHandlers.Add("metadata");
        }
        private readonly ConcurrentBag<string> _messageHandlers = new ConcurrentBag<string>();

        public async Task HandleMessageAsync(string clientId, SDPMessage message, IWebSocketManager webSocket)
        {


            try
            {

                switch (message.Type)
                {
                    case "metadata":
                        await HandleMetadataMessage(clientId, message);
                        break;
                    case "offer":
                        var offer = await _sdpHandler.ProcessOfferAsync(message);
                        if (offer != null) await webSocket.BroadcastMessageAsync(offer.Sdp);
                        break;
                    case "answer":
                        var answer = await _sdpHandler.ProcessAnswerAsync(message);
                        if (answer != null) await webSocket.BroadcastMessageAsync(answer.Sdp);
                        break;
                    case "candidate":
                        var candidate = await _sdpHandler.ProcessCandidateAsync(message);
                        if (candidate != null) await webSocket.BroadcastMessageAsync(candidate.Sdp);
                        break;
                    default:
                        _logger.LogWarning($"Unknown message type");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mesaj işleme hatası");
            }
        }

        private async Task HandleMetadataMessage(string clientId, SDPMessage message)
        {
            var metadata = JsonConvert.DeserializeObject<PeerMetadata>(message.Payload);
            metadata.ClientId = clientId;
            await _peerDiscoveryService.UpdatePeerMetadataAsync(metadata);
        }

        public Task<bool> isProccesType(string typename)
        {
            return _messageHandlers.Contains(typename) ? Task.FromResult(true) : Task.FromResult(false);
        }
    }
}
