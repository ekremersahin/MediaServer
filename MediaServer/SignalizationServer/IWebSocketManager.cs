using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer
{
    public interface IWebSocketManager
    {
        Task ManageWebSocketConnectionAsync(WebSocket webSocket);
        Task BroadcastMessageAsync(string message);
        Task SendMessageToClientAsync(string clientId, string message);
        Task<string> RegisterClientAsync(WebSocket webSocket);
        Task RemoveClientAsync(string clientId);
        WebSocket GetWebSocketClient(string clientId);
    }
}
