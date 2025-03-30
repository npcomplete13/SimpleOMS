using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;

namespace BroadcastService.Middleware
{
    public class BroadcastSocketManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public string AddSocket(WebSocket socket)
        {
            var socketId = Guid.NewGuid().ToString();
            _sockets.TryAdd(socketId, socket);
            return socketId;
        }

        public async Task RemoveSocket(string socketId)
        {
            if (_sockets.TryRemove(socketId, out var socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the BroadcastSocketManager", CancellationToken.None);
                socket.Dispose();
            }
        }

        public async Task SendMessageToAllAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var tasks = _sockets.Values.Select(socket => socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
            await Task.WhenAll(tasks);
        }
    }
}
