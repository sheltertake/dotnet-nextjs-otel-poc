namespace MyApiChat;
public static class ChatWebSocketMiddlewareExtensions
{
    public static IApplicationBuilder UseChatWebSocket(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ChatWebSocketMiddleware>();
    }
}