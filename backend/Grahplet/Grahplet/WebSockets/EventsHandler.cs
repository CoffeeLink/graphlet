using System.Net.WebSockets;

namespace Grahplet.WebSockets;

public class EventsHandler
{
    public async Task HandleAsync(HttpContext ctx, WebSocket socket)
    {
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, ":3", CancellationToken.None);
    }
}
