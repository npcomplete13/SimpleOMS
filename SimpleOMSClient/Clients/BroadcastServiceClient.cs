using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace SimpleOMSClient.Clients
{
    public class BroadcastServiceClient : IDisposable
    {
        private readonly ClientWebSocket _clientWebSocket;
        private Uri _serverUri;
        private string _serverUriString;

        public event EventHandler<string> OnMessageReceived = delegate { };

        public BroadcastServiceClient(string serverUri)
        {
            _clientWebSocket = new ClientWebSocket();
            _serverUriString = serverUri;
        }

        public async Task ConnectAsync(string name)
        {
            _serverUri = new Uri(_serverUriString + "/ws?name=" + name);
            await _clientWebSocket.ConnectAsync(_serverUri, CancellationToken.None);
            Console.WriteLine("Connected to the WebSocket server.");
            await ReceiveMessagesAsync();
        }

        public async Task SendMessageAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(messageBytes);
            await _clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent: {message}");
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024 * 4];
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    Console.WriteLine("WebSocket connection closed.");
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                    OnMessageReceived(this, message);
                }
            }
        }

        public async Task DisconnectAsync()
        {
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
            Console.WriteLine("Disconnected from the WebSocket server.");
        }

        public void Dispose()
        {
            _clientWebSocket.Dispose();
        }
    }
}
