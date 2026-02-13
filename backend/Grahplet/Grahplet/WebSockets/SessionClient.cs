using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Grahplet.Repositories;

namespace Grahplet.WebSockets;

/// <summary>
/// Handles a single WebSocket connection lifecycle.
/// Each instance represents one client connection.
/// </summary>
public class SessionClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly SessionDictionary _sessionDictionary;
    private readonly Guid _connectionId = Guid.NewGuid();

    private PeriodicTimer _pingTimer = new(TimeSpan.FromSeconds(5));
    private DateTime _lastPingSent = DateTime.UtcNow;
    private bool _waitingForPong = false;

    // Channels for communication with LiveSession
    private Channel<WebSocketMessage>? _clientTx;
    private Channel<object>? _sessionRx;

    private Guid? _userId;
    private Guid? _workspaceId;

    public SessionClient(IServiceProvider serviceProvider, SessionDictionary sessionDictionary)
    {
        _serviceProvider = serviceProvider;
        _sessionDictionary = sessionDictionary;
    }

    public async Task HandleAsync(HttpContext ctx, WebSocket socket)
    {
        try
        {
            // Step 1: Authenticate
            if (!await AuthenticateAsync(ctx, socket))
            {
                return;
            }

            // Step 2: Connect to LiveSession
            var liveSession = _sessionDictionary.GetOrCreateSession(_workspaceId!.Value);
            _clientTx = Channel.CreateUnbounded<WebSocketMessage>();
            _sessionRx = liveSession.Rx;

            // Send connect message to LiveSession
            var connectMsg = new ClientConnectInternal(_connectionId, _userId!.Value, _clientTx);
            await _sessionRx.Writer.WriteAsync(connectMsg, ctx.RequestAborted);

            // Step 3: Wait for SessionInfo from LiveSession
            if (await _clientTx.Reader.WaitToReadAsync(ctx.RequestAborted))
            {
                if (_clientTx.Reader.TryRead(out var sessionInfo))
                {
                    await SendMessageAsync(socket, sessionInfo, ctx.RequestAborted);
                }
            }

            // Step 4: Run the message loop
            await RunMessageLoopAsync(ctx, socket);
        }
        finally
        {
            // Cleanup: notify LiveSession that we're disconnecting
            if (_sessionRx != null && _userId.HasValue)
            {
                var disconnectMsg = new ClientDisconnectInternal(_connectionId, _userId.Value);
                try
                {
                    await _sessionRx.Writer.WriteAsync(disconnectMsg, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending disconnect message: {ex.Message}");
                }
            }
        }
    }

    private async Task<bool> AuthenticateAsync(HttpContext ctx, WebSocket socket)
    {
        // Receive first message (must be Auth)
        var firstMessage = await ReceiveMessageAsync(socket, ctx.RequestAborted);
        if (firstMessage == null)
        {
            await CloseAsync(socket, WebSocketCloseStatus.PolicyViolation, "Expected Auth message");
            return false;
        }

        if (firstMessage is not AuthMessage auth)
        {
            await CloseAsync(socket, WebSocketCloseStatus.PolicyViolation, "First message must be Auth");
            return false;
        }

        if (string.IsNullOrWhiteSpace(auth.Token) || auth.Workspace == Guid.Empty)
        {
            await CloseAsync(socket, WebSocketCloseStatus.PolicyViolation, "Invalid Auth payload");
            return false;
        }

        // Validate token and workspace access
        using var scope = _serviceProvider.CreateScope();
        var authRepository = scope.ServiceProvider.GetRequiredService<IAuthRepository>();
        var workspaceRepository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();

        var userId = await authRepository.GetUserIdFromTokenAsync(auth.Token);
        if (userId == null)
        {
            await CloseAsync(socket, WebSocketCloseStatus.PolicyViolation, "Invalid token");
            return false;
        }

        var workspace = await workspaceRepository.GetWorkspaceAsync(userId.Value, auth.Workspace);
        if (workspace == null)
        {
            await CloseAsync(socket, WebSocketCloseStatus.PolicyViolation, "Workspace not found or access denied");
            return false;
        }

        _userId = userId.Value;
        _workspaceId = auth.Workspace;

        Console.WriteLine($"[SessionClient {_connectionId}] Authenticated: User={_userId}, Workspace={_workspaceId}");
        return true;
    }

    private async Task RunMessageLoopAsync(HttpContext ctx, WebSocket socket)
    {
        while (socket.State == WebSocketState.Open && !ctx.RequestAborted.IsCancellationRequested)
        {
            using var pingTimerCts = CancellationTokenSource.CreateLinkedTokenSource(ctx.RequestAborted);

            var recvWebSocket = ReceiveMessageAsync(socket, ctx.RequestAborted);
            var recvChannel = _clientTx!.Reader.WaitToReadAsync(ctx.RequestAborted).AsTask();
            var pingTimerTask = _pingTimer.WaitForNextTickAsync(pingTimerCts.Token).AsTask();

            var completed = await Task.WhenAny(recvWebSocket, recvChannel, pingTimerTask);

            if (completed == recvWebSocket)
            {
                pingTimerCts.Cancel();
                var message = await recvWebSocket;
                if (message == null)
                {
                    Console.WriteLine($"[SessionClient {_connectionId}] Client disconnected");
                    break;
                }

                await HandleClientMessageAsync(socket, message, ctx.RequestAborted);
            }
            else if (completed == recvChannel)
            {
                pingTimerCts.Cancel();
                if (await recvChannel)
                {
                    // Read all available messages from LiveSession
                    while (_clientTx!.Reader.TryRead(out var msg))
                    {
                        await SendMessageAsync(socket, msg, ctx.RequestAborted);
                    }
                }
            }
            else if (completed == pingTimerTask)
            {
                await HandlePingTimerAsync(socket, ctx.RequestAborted);
            }
        }
    }

    private async Task HandleClientMessageAsync(WebSocket socket, WebSocketMessage message, CancellationToken ct)
    {
        switch (message)
        {
            case PongMessage:
                _waitingForPong = false;
                Console.WriteLine($"[SessionClient {_connectionId}] Received Pong");
                break;

            case PingMessage:
                await SendMessageAsync(socket, new PongMessage(), ct);
                break;

            case DisconnectMessage:
                Console.WriteLine($"[SessionClient {_connectionId}] Client requested disconnect");
                await CloseAsync(socket, WebSocketCloseStatus.NormalClosure, "Client requested disconnect");
                break;

            case LockAction or UnlockAction or CustomClientAction:
                // Relay to LiveSession
                var actionInternal = new ClientActionInternal(_connectionId, _userId!.Value, message);
                await _sessionRx!.Writer.WriteAsync(actionInternal, ct);
                break;

            default:
                Console.WriteLine($"[SessionClient {_connectionId}] Unknown message type: {message.ToString()}");
                break;
        }
    }

    private async Task HandlePingTimerAsync(WebSocket socket, CancellationToken ct)
    {
        if (_waitingForPong)
        {
            var timeSinceLastPing = DateTime.UtcNow - _lastPingSent;
            if (timeSinceLastPing > TimeSpan.FromSeconds(10))
            {
                Console.WriteLine($"[SessionClient {_connectionId}] Ping timeout, closing connection");
                await CloseAsync(socket, WebSocketCloseStatus.NormalClosure, "Ping timeout");
                return;
            }
        }

        try
        {
            await SendMessageAsync(socket, new PingMessage(), ct);
            _waitingForPong = true;
            _lastPingSent = DateTime.UtcNow;
            Console.WriteLine($"[SessionClient {_connectionId}] Sent Ping");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionClient {_connectionId}] Error sending ping: {ex.Message}");
        }
    }

    private async Task<WebSocketMessage?> ReceiveMessageAsync(WebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[8 * 1024];
        var segment = new ArraySegment<byte>(buffer);
        using var ms = new MemoryStream();

        while (true)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(segment, ct);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (WebSocketException)
            {
                return null;
            }

            if (result.MessageType == WebSocketMessageType.Close)
                return null;

            ms.Write(segment.Array!, segment.Offset, result.Count);

            if (result.EndOfMessage)
                break;

            if (ms.Length > 256 * 1024)
                return null;
        }

        var json = Encoding.UTF8.GetString(ms.ToArray());
        try
        {
            return JsonSerializer.Deserialize<WebSocketMessage>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionClient {_connectionId}] Error deserializing message: {ex.Message}");
            Console.WriteLine($"[SessionClient {_connectionId}] Payload was: {json}");
            return null;
        }
    }

    private async Task SendMessageAsync(WebSocket socket, object message, CancellationToken ct)
    {
        try
        {
            var json = JsonSerializer.Serialize(message, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionClient {_connectionId}] Error sending message: {ex.Message}");
        }
    }

    private async Task CloseAsync(WebSocket socket, WebSocketCloseStatus status, string reason)
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            try
            {
                await socket.CloseAsync(status, reason, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionClient {_connectionId}] Error closing socket: {ex.Message}");
            }
        }
    }
}
