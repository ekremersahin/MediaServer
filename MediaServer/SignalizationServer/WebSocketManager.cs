using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using MediaServer.Media.Interfaces;
using MediaServer.RTC.Models;
using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using MediaServer.SignalizationServer;
using MediaServer.SignalizationServer.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer
{
    public class WebSocketManager : IWebSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();
        private readonly ILogger<WebSocketManager> _logger;

        private readonly ISDPHandler _sdpHandler;
        private readonly ITelemetryService telemetryService;
        private readonly IEnumerable<IWebSocketMediator> webSocketMediators;

        public WebSocketManager(
            ILogger<WebSocketManager> logger,
            ISDPHandler sdpHandler,
            ITelemetryService telemetryService,
            IEnumerable<IWebSocketMediator> webSocketMediators
            )
        {
            _logger = logger;
            _sdpHandler = sdpHandler;
            this.telemetryService = telemetryService;
            this.webSocketMediators = webSocketMediators;
        }
        public async Task<string> RegisterClientAsync(WebSocket webSocket)
        {
            var clientId = Guid.NewGuid().ToString();

            if (_clients.TryAdd(clientId, webSocket))
            {
                _logger.LogInformation($"Client {clientId} registered");
                _ = telemetryService.TrackMetricAsync(new TrackingModel()
                {
                    MetricName = "ClientRegistered",
                    Value = 1,
                    Properties = new Dictionary<string, string> { { "ClientId", clientId } },
                    Timestamp = DateTime.UtcNow
                });
                return clientId;
            }

            _ = telemetryService.TrackErrorAsync(new TrackingModel()
            {
                Value = 1,
                MetricName = Environment.MachineName,
                Properties = new Dictionary<string, string> { { "ClientId", clientId } },
                Timestamp = DateTime.UtcNow
            }, null);

            return null;
        }

        public async Task RemoveClientAsync(string clientId)
        {
            if (_clients.TryRemove(clientId, out var webSocket))
            {
                try
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed",
                            CancellationToken.None
                        );
                    }
                    _logger.LogInformation($"Client {clientId} removed");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error removing client {clientId}: {ex.Message}");
                }
            }
        }

        public async Task BroadcastMessageAsync(string message)
        {
            var tasks = _clients.Values
                .Where(ws => ws.State == WebSocketState.Open)
                .Select(async ws =>
                {
                    try
                    {
                        await ws.SendAsync(
                            new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Broadcast error: {ex.Message}");
                    }
                });

            await Task.WhenAll(tasks);
        }

        public async Task SendMessageToClientAsync(string clientId, string message)
        {
            if (_clients.TryGetValue(clientId, out var webSocket))
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
                else
                {
                    _logger.LogWarning($"Cannot send message. Client {clientId} is not in open state.");
                }
            }
            else
            {
                _logger.LogWarning($"Client {clientId} not found");
            }
        }

        public async Task ManageWebSocketConnectionAsync(WebSocket webSocket)
        {
            var clientId = await RegisterClientAsync(webSocket);
            if (clientId != null)
            {

                var messageStream = new MemoryStream();
                try
                {

                    while (webSocket.State == WebSocketState.Open)
                    {
                        var result = await ReceiveMessageAsync(webSocket, messageStream);

                        if (result.Item1.MessageType == WebSocketMessageType.Close)
                        {
                            await RemoveClientAsync(clientId);
                            break;
                        }

                        if (result.Item1.MessageType == WebSocketMessageType.Text)
                        {
                            await HandleReceivedMessageAsync(clientId, result.Item1, result.Item2);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"WebSocket error for client {clientId}: {ex.Message}");

                }
                finally
                {
                    await RemoveClientAsync(clientId);
                }
            }

        }

        private async Task<(WebSocketReceiveResult, MemoryStream)> ReceiveMessageAsync(WebSocket webSocket, MemoryStream messageStream)
        {
            var buffer = new byte[64];

            WebSocketReceiveResult result;
            messageStream.SetLength(0); // Stream'i temizle

            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.Count > 0)
                    await messageStream.WriteAsync(buffer, 0, result.Count);

            } while (!result.EndOfMessage);


            return (result, messageStream);
        }
        private async Task HandleReceivedMessageAsync(string clientId, WebSocketReceiveResult result, MemoryStream messageStream)
        {
            var buffer = messageStream.ToArray();

            var message = Encoding.UTF8.GetString(messageStream.ToArray(), 0, buffer.Length);

            try
            {
                var sdpMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<SDPMessage>(message);
                sdpMessage.ClientId = clientId;
                // SDP mesajını parse et
                //var sdpMessage = new SDPMessage
                //{
                //    Sdp = message,
                //    ClientId = clientId,
                //    Type = DetermineSDPMessageType(message)
                //};
                bool resume = true;
                foreach (var item in webSocketMediators)
                {
                    if (await item.isProccesType(sdpMessage.Type))
                    {
                        await item.HandleMessageAsync(sdpMessage.ClientId, sdpMessage, this);
                        resume = false;
                        break;
                    }
                }
                if (resume)
                {
                    switch (sdpMessage.Type)
                    {
                        case "offer":
                            var offer = await _sdpHandler.ProcessOfferAsync(sdpMessage);
                            if (offer != null) await this.BroadcastMessageAsync(offer.Sdp);
                            break;
                        case "answer":
                            var answer = await _sdpHandler.ProcessAnswerAsync(sdpMessage);
                            if (answer != null) await this.BroadcastMessageAsync(answer.Sdp);
                            break;
                        case "candidate":
                            var candidate = await _sdpHandler.ProcessCandidateAsync(sdpMessage);
                            if (candidate != null) await this.BroadcastMessageAsync(candidate.Sdp);
                            break;
                        case "media":
                            // Media için yeni bir işlem gerekli
                            await ProcessMediaMessageAsync(sdpMessage);
                            break;
                        default:
                            _logger.LogWarning($"Unknown message type");
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }
        private string DetermineSDPMessageType(string sdp)
        {
            // SDP içeriğine göre mesaj türünü belirle
            if (sdp.Contains("a=group:BUNDLE"))
                return "offer";
            else if (sdp.Contains("a=setup:active"))
                return "answer";
            else if (sdp.Contains("candidate:"))
                return "candidate";

            return "unknown";
        }


        private async Task ProcessMediaMessageAsync(SDPMessage mediaMessage)
        {
            // Media işleme logic'i
            // Örneğin:
            // 1. Media içeriğini parse et
            // 2. Gerekli mediaHandler'a gönder
            // 3. Routing işlemleri yap

            // Şimdilik basit bir log
            _logger.LogInformation($"Processing media message from client {mediaMessage.ClientId}");

            // Gelecekte detaylandırılacak
            await Task.CompletedTask;
        }

        public WebSocket GetWebSocketClient(string clientId)
        {
            if (this._clients.TryGetValue(clientId, out var webSocket))
            {
                return webSocket;
            };
            return null;
        }
    }
}