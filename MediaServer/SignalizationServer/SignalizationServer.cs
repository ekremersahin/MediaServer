//using MediaServer.SDP.Interfaces;
//using MediaServer.SignalizationServer;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Concurrent;
//using System.ComponentModel.DataAnnotations;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MediaServer.SignalizationServer
//{
//    public class SignalizationServer
//    {
//        private readonly IWebSocketManager _webSocketManager;
//        private readonly SDPManager _sdpManager;

//        public SignalizationServer(
//            IWebSocketManager webSocketManager,
//            SDPManager sdpManager)
//        {
//            _webSocketManager = webSocketManager;
//            _sdpManager = sdpManager;
//        }

//        public async Task HandleWebSocketConnection(WebSocket webSocket)
//        {
//            try
//            {
//                await _webSocketManager.ManageWebSocketConnectionAsync(webSocket);
//            }
//            catch (Exception)
//            {

//            }

//        }
//    }

//    //public class SignalizationServer
//    //{
//    //    private readonly IWebSocketManager _webSocketManager;
//    //    private readonly SDPManager _sdpManager;
//    //    private readonly IConfiguration _configuration;

//    //    private readonly ConcurrentDictionary<string, WebSocket> _webSockets = new ConcurrentDictionary<string, WebSocket>();

//    //    private readonly ISDPProcessor _sdpProcessor;
//    //    private readonly ISDPParser sDPParser;
//    //    private readonly ISDPValidator sDPValidator;

//    //    public SignalizationServer(
//    //        IConfiguration configuration,
//    //        IWebSocketManager webSocketManager,
//    //        SDPManager sdpManager)
//    //    {
//    //        _configuration = configuration;
//    //        _webSocketManager = webSocketManager;
//    //        _sdpManager = sdpManager;
//    //    }

//    //    public async Task ManageWebSocket(WebSocket webSocket)
//    //    {
//    //        var connectionId = Guid.NewGuid().ToString();
//    //        _webSockets[connectionId] = webSocket;

//    //        var buffer = new byte[4096]; // Başlangıç buffer boyutu
//    //        var messageStream = new MemoryStream();

//    //        try
//    //        {
//    //            while (webSocket.State == WebSocketState.Open)
//    //            {
//    //                messageStream.SetLength(0);
//    //                WebSocketReceiveResult result;
//    //                do
//    //                {
//    //                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

//    //                    if (result.Count > 0)
//    //                        await messageStream.WriteAsync(buffer, 0, result.Count);

//    //                } while (!result.EndOfMessage);

//    //                if (result.MessageType == WebSocketMessageType.Close)
//    //                {
//    //                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
//    //                        "Closing connection", CancellationToken.None);
//    //                    break;
//    //                }
//    //                if (result.MessageType == WebSocketMessageType.Text)
//    //                {
//    //                    var message = Encoding.UTF8.GetString(messageStream.ToArray());
//    //                    _sdpManager.ProcessSDPMessage(message, webSocket);
//    //                    break;
//    //                }



//    //                // var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

//    //                //if (received.MessageType == WebSocketMessageType.Text)
//    //                //{
//    //                //    var message = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);

//    //                //    Console.WriteLine($"Alınan mesaj: {message}");

//    //                //    // Mesajı işleyin (örneğin, SDP mesajlarını işleyin)
//    //                //    _sdpManager.ProcessSDPMessage(message, webSocket);

//    //                //    // Mesajı diğer istemcilere yayınlayın
//    //                //    foreach (var otherWebSocket in _webSockets.Values)
//    //                //    {
//    //                //        if (otherWebSocket.State == WebSocketState.Open)
//    //                //        {
//    //                //            await otherWebSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
//    //                //        }
//    //                //    }
//    //                //}
//    //                //else if (received.MessageType == WebSocketMessageType.Close)
//    //                //{
//    //                //    break;
//    //                //}
//    //            }
//    //        }
//    //        finally
//    //        {
//    //            messageStream.Dispose();
//    //            //ManageWebSocket(webSocket);
//    //            //if (webSocket.State != WebSocketState.Closed)
//    //            //{
//    //            //    webSocket.Abort();
//    //            //}

//    //            //_webSockets.Remove(connectionId, out _);
//    //            //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
//    //        }
//    //    }




//    //}
//}