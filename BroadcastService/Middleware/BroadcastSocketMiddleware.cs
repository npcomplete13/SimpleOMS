using System.Net.WebSockets;
using System.Text;

namespace BroadcastService.Middleware
{
    public class BroadcastSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BroadcastSocketManager _socketManager;

        public BroadcastSocketMiddleware(RequestDelegate next, BroadcastSocketManager socketManager)
        {
            _next = next;
            _socketManager = socketManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var socketId = _socketManager.AddSocket(socket);

                await Receive(socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await _socketManager.SendMessageToAllAsync(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _socketManager.RemoveSocket(socketId);
                    }
                });
            }
            else
            {
                await _next(context);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }
    }
}
