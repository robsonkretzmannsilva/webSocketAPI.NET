using System;
using System.Net.WebSockets;

namespace WebSocketPool.Domain.Interface
{
	public interface IWebSocketPool
	{
        void AddSocket(string id, WebSocket socket);

        Task<string> HandleWebSocketConnection(string id, WebSocket webSocket);
    }
}

