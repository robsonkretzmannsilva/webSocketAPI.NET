using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using WebSocketPool.Domain.Interface;
namespace WebSocketPool.Service.Service
{
	public class WebSocketPool: IWebSocketPool
	{
        private readonly ConcurrentDictionary<string, List<WebSocket>> _pools = new ConcurrentDictionary<string, List<WebSocket>>();

        public void AddSocket(string id, WebSocket socket)
        {
            _pools.AddOrUpdate(id, new List<WebSocket> { socket }, (key, existingList) =>
            {
                existingList.Add(socket);
                return existingList;
            });
        }

        private void RemoveSocket(string id, WebSocket socket)
        {
            if (_pools.TryGetValue(id, out var sockets))
            {
                sockets.Remove(socket);
                if (sockets.Count == 0)
                {
                    _pools.TryRemove(id, out _);
                }
            }
        }

        private IEnumerable<WebSocket> GetSockets(string id)
        {
            if (_pools.TryGetValue(id, out var sockets))
            {
                return sockets;
            }
            return Enumerable.Empty<WebSocket>();
        }

        public async Task<string> HandleWebSocketConnection(string id, WebSocket webSocket)
        {
            string receivedMessage = string.Empty;
            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result;

                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await BroadcastMessage(id, receivedMessage);
                    }
                } while (!result.CloseStatus.HasValue);

                RemoveSocket(id, webSocket);
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Handle errors
                RemoveSocket(id, webSocket);
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            }
            return receivedMessage;
        }

        private async Task BroadcastMessage(string id, string message)
        {
            var sockets = GetSockets(id);
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            foreach (var socket in sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}

