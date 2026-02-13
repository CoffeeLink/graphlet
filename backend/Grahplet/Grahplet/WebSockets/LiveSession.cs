using System.Threading.Channels;

namespace Grahplet.WebSockets;

/// <summary>
/// Manages a single workspace's live session.
/// Handles connected clients, locks, and relays client actions.
/// </summary>
public class LiveSession
{
    private readonly Guid _workspaceId;

    // Communication channel - clients send messages here
    public Channel<object> Rx { get; } = Channel.CreateUnbounded<object>();

    // Connected clients: ConnectionId -> (UserId, ClientTx)
    private readonly Dictionary<Guid, (Guid UserId, Channel<WebSocketMessage> ClientTx)> _clients = new();

    // Locks: NoteId -> UserId (who locked it)
    private readonly Dictionary<Guid, Guid> _locks = new();

    // Track user connections: UserId -> ConnectionId (latest connection)
    private readonly Dictionary<Guid, Guid> _userConnections = new();

    public LiveSession(Guid workspaceId)
    {
        _workspaceId = workspaceId;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        Console.WriteLine($"[LiveSession {_workspaceId}] Started");

        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (await Rx.Reader.WaitToReadAsync(ct))
                {
                    while (Rx.Reader.TryRead(out var message))
                    {
                        await ProcessMessageAsync(message, ct);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[LiveSession {_workspaceId}] Cancelled");
        }
        finally
        {
            Console.WriteLine($"[LiveSession {_workspaceId}] Stopped");
            Rx.Writer.Complete();
        }
    }

    private async Task ProcessMessageAsync(object message, CancellationToken ct)
    {
        switch (message)
        {
            case ClientConnectInternal connect:
                await HandleClientConnectAsync(connect, ct);
                break;

            case ClientDisconnectInternal disconnect:
                await HandleClientDisconnectAsync(disconnect, ct);
                break;

            case ClientActionInternal action:
                await HandleClientActionAsync(action, ct);
                break;

            default:
                Console.WriteLine($"[LiveSession {_workspaceId}] Unknown message type: {message.GetType().Name}");
                break;
        }
    }

    private async Task HandleClientConnectAsync(ClientConnectInternal connect, CancellationToken ct)
    {
        // Check if this user already has a connection - kick off the old one
        if (_userConnections.TryGetValue(connect.UserId, out var oldConnectionId))
        {
            if (_clients.TryGetValue(oldConnectionId, out var oldClient))
            {
                Console.WriteLine($"[LiveSession {_workspaceId}] Kicking off old connection {oldConnectionId} for user {connect.UserId}");

                // Send error to old client and remove it
                try
                {
                    await oldClient.ClientTx.Writer.WriteAsync(
                        new ServerErrorEvent("New connection from same user"), ct);
                }
                catch { }

                _clients.Remove(oldConnectionId);
            }
        }

        // Add new client
        _clients[connect.ConnectionId] = (connect.UserId, connect.ClientTx);
        _userConnections[connect.UserId] = connect.ConnectionId;

        Console.WriteLine($"[LiveSession {_workspaceId}] Client connected: ConnectionId={connect.ConnectionId}, UserId={connect.UserId}, Total={_clients.Count}");

        // Send SessionInfo to the new client
        var sessionInfo = new SessionInfoMessage(
            Users: _clients.Values.Select(c => c.UserId).Distinct().ToList(),
            Locks: new Dictionary<Guid, Guid>(_locks)
        );

        try
        {
            await connect.ClientTx.Writer.WriteAsync(sessionInfo, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LiveSession {_workspaceId}] Error sending SessionInfo: {ex.Message}");
        }

        // Broadcast UserJoined to all other clients
        await BroadcastAsync(new UserJoinedEvent(connect.UserId), excludeConnectionId: connect.ConnectionId, ct);
    }

    private async Task HandleClientDisconnectAsync(ClientDisconnectInternal disconnect, CancellationToken ct)
    {
        if (!_clients.Remove(disconnect.ConnectionId))
        {
            return; // Client not found
        }

        // Remove from user connections if this was the active connection
        if (_userConnections.TryGetValue(disconnect.UserId, out var activeConnectionId)
            && activeConnectionId == disconnect.ConnectionId)
        {
            _userConnections.Remove(disconnect.UserId);
        }

        // Remove any locks held by this user
        var locksToRemove = _locks.Where(kvp => kvp.Value == disconnect.UserId).Select(kvp => kvp.Key).ToList();
        foreach (var noteId in locksToRemove)
        {
            _locks.Remove(noteId);
        }

        Console.WriteLine($"[LiveSession {_workspaceId}] Client disconnected: ConnectionId={disconnect.ConnectionId}, UserId={disconnect.UserId}, Total={_clients.Count}");

        // Broadcast UserLeft to all remaining clients
        await BroadcastAsync(new UserLeftEvent(disconnect.UserId), excludeConnectionId: null, ct);
    }

    private async Task HandleClientActionAsync(ClientActionInternal actionInternal, CancellationToken ct)
    {
        var action = actionInternal.Action;

        // Handle lock/unlock actions
        switch (action)
        {
            case LockAction lockAction:
                // Check if already locked by someone else
                if (_locks.TryGetValue(lockAction.NoteId, out var lockHolder) && lockHolder != actionInternal.UserId)
                {
                    // Already locked by someone else - send error to requesting client
                    if (_clients.TryGetValue(actionInternal.ConnectionId, out var client))
                    {
                        try
                        {
                            await client.ClientTx.Writer.WriteAsync(
                                new ServerErrorEvent($"Note {lockAction.NoteId} is already locked"), ct);
                        }
                        catch { }
                    }
                    return;
                }

                // Lock it
                _locks[lockAction.NoteId] = actionInternal.UserId;
                Console.WriteLine($"[LiveSession {_workspaceId}] Note {lockAction.NoteId} locked by user {actionInternal.UserId}");
                break;

            case UnlockAction unlockAction:
                // Only the lock holder can unlock
                if (_locks.TryGetValue(unlockAction.NoteId, out var holder) && holder == actionInternal.UserId)
                {
                    _locks.Remove(unlockAction.NoteId);
                    Console.WriteLine($"[LiveSession {_workspaceId}] Note {unlockAction.NoteId} unlocked by user {actionInternal.UserId}");
                }
                break;
        }

        // Broadcast the action to all other clients (cast to WebSocketMessage for broadcasting)
        if (action is WebSocketMessage wsMessage)
        {
            await BroadcastAsync(wsMessage, excludeConnectionId: actionInternal.ConnectionId, ct);
        }
    }

    private async Task BroadcastAsync(WebSocketMessage message, Guid? excludeConnectionId, CancellationToken ct)
    {
        var clients = _clients.Where(kvp => kvp.Key != excludeConnectionId).ToList();

        foreach (var (connectionId, (userId, clientTx)) in clients)
        {
            try
            {
                await clientTx.Writer.WriteAsync(message, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LiveSession {_workspaceId}] Error broadcasting to {connectionId}: {ex.Message}");
            }
        }
    }

    public int ClientCount => _clients.Count;
}
