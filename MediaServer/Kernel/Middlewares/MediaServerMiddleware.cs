
using MediaServer.SignalizationServer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MediaServer.Kernel.Middlewares
{
    public sealed class MediaServerMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly IWebSocketManager webSocketManager;

        public MediaServerMiddleware(RequestDelegate next, IWebSocketManager webSocketManager)
        {
            _next = next;
            this.webSocketManager = webSocketManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // İstek öncesi işlemler

            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    try
                    {

                        await webSocketManager.ManageWebSocketConnectionAsync(webSocket);

                        //await signalizationServer.HandleWebSocketConnection(webSocket);
                    }
                    catch (Exception ex)
                    {

                    }

                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }

            // İstek sonrası işlemler
        }

    }
}
