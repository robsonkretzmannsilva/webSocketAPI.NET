using System;
using WebSocketPool.Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebSocketPool.Service.Service;

namespace Websocket.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
		private IWebSocketPool _websocket;
		public ChatController(IWebSocketPool webSocket)
		{
			_websocket = webSocket;
		}

        [HttpGet]
        [Route("sendMessage/{chatId}")]
        [AllowAnonymous]
        public async Task SendMessage(string chatId)
		{
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            _websocket.AddSocket(chatId, webSocket);

            string receivedMessages = await _websocket.HandleWebSocketConnection(chatId, webSocket);
        }
	}
}

