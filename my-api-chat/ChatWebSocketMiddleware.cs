using Datadog.Trace;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace MyApiChat;
public class ChatWebSocketMiddleware
{
    private static ConcurrentBag<string> _messages = new ConcurrentBag<string>();
    // Struttura per tenere traccia dei client connessi
    private static ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();
    private readonly RequestDelegate _next;
    private readonly ILogger<ChatWebSocketMiddleware> _logger;

    public ChatWebSocketMiddleware(RequestDelegate next, ILogger<ChatWebSocketMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            string clientId = Guid.NewGuid().ToString();
            var activeScope = Tracer.Instance.ActiveScope;

            using (var scope = Tracer.Instance.StartActive($"HandleConnection {clientId}"))
            {
                _clients.TryAdd(clientId, webSocket);
            
                _logger.LogInformation("New connection: {ClientId}", clientId);

                await SendStoredMessages(webSocket);
            }

            //activeScope.Span.ResourceName = $"GET /";
            //// close the OperationName: aspnet_core.request
            //// Resource: GET /
            //activeScope.Close();

            _logger.LogInformation("Connection - Active scoped closed and span disposed");

            await HandleWebSocketAsync(webSocket, clientId);
            
        }
        else
        {
            // Non-WebSocket requests
            await _next(context);
        }
    }

    private async Task SendStoredMessages(WebSocket webSocket)
    {
        foreach (var message in _messages)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(messageBytes, 0, messageBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async Task HandleWebSocketAsync(WebSocket webSocket, string clientId)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            using (var scope = Tracer.Instance.StartActive($"HandleMessage {message}"))
            {
                _logger.LogInformation("Received message: {Message}", message);

            }
            //Tracer.Instance.ActiveScope?.Close();
            _logger.LogInformation("Active scoped closed and span disposed");


            _messages.Add(message);
            await BroadcastMessage(message, clientId);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        _clients.TryRemove(clientId, out _);
    }

    private async Task BroadcastMessage(string message, string senderId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        foreach (var clientId in _clients.Keys)
        {

            WebSocket clientSocket;
            if (_clients.TryGetValue(clientId, out clientSocket))
            {
                if (clientSocket.State == WebSocketState.Open)
                {
                    await clientSocket.SendAsync(new ArraySegment<byte>(messageBytes, 0, messageBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
