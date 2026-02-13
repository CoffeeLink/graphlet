using System.Text.Json.Serialization;

namespace Grahplet.WebSockets;

/// <summary>
/// Base class for all WebSocket messages
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PingMessage), typeDiscriminator: "Ping")]
[JsonDerivedType(typeof(PongMessage), typeDiscriminator: "Pong")]
[JsonDerivedType(typeof(AuthMessage), typeDiscriminator: "Auth")]
[JsonDerivedType(typeof(DisconnectMessage), typeDiscriminator: "Disconnect")]
[JsonDerivedType(typeof(SessionInfoMessage), typeDiscriminator: "SessionInfo")]
[JsonDerivedType(typeof(MessageAckMessage), typeDiscriminator: "MessageAck")]
[JsonDerivedType(typeof(UserJoinedEvent), typeDiscriminator: "UserJoined")]
[JsonDerivedType(typeof(UserLeftEvent), typeDiscriminator: "UserLeft")]
[JsonDerivedType(typeof(ServerErrorEvent), typeDiscriminator: "ServerError")]
[JsonDerivedType(typeof(LockAction), typeDiscriminator: "Lock")]
[JsonDerivedType(typeof(UnlockAction), typeDiscriminator: "Unlock")]
[JsonDerivedType(typeof(CustomClientAction), typeDiscriminator: "Custom")]
public abstract record WebSocketMessage;

// ============================================================================
// SYSTEM EVENTS - Server-to-Client system-level messages
// ============================================================================

public record PingMessage() : WebSocketMessage;

public record PongMessage() : WebSocketMessage;

public record AuthMessage(string Token, Guid Workspace) : WebSocketMessage;

public record DisconnectMessage() : WebSocketMessage;

public record SessionInfoMessage(List<Guid> Users, Dictionary<Guid, Guid> Locks) : WebSocketMessage;

public record MessageAckMessage(Guid MessageId) : WebSocketMessage;

// ============================================================================
// SESSION EVENTS - Server-to-Client session-level events
// ============================================================================

public record UserJoinedEvent(Guid UserId) : WebSocketMessage;

public record UserLeftEvent(Guid UserId) : WebSocketMessage;

public record ServerErrorEvent(string Message) : WebSocketMessage;

// ============================================================================
// CLIENT ACTIONS - Client-to-Server and Server-to-Client actions
// ============================================================================

public record LockAction(Guid UserId, Guid NoteId) : WebSocketMessage;

public record UnlockAction(Guid UserId, Guid NoteId) : WebSocketMessage;

public record CustomClientAction(Guid UserId, string CustomType, object? Payload) : WebSocketMessage;

// ============================================================================
// INTERNAL MESSAGES - Used only on the server side between components
// ============================================================================

/// <summary>
/// Internal message sent from SessionClient to LiveSession when a client connects
/// </summary>
internal record ClientConnectInternal(
    Guid ConnectionId,
    Guid UserId,
    System.Threading.Channels.Channel<WebSocketMessage> ClientTx
);

/// <summary>
/// Internal message sent from SessionClient to LiveSession when a client disconnects
/// </summary>
internal record ClientDisconnectInternal(
    Guid ConnectionId,
    Guid UserId
);

/// <summary>
/// Internal message sent from SessionClient to LiveSession for client actions
/// </summary>
internal record ClientActionInternal(
    Guid ConnectionId,
    Guid UserId,
    WebSocketMessage Action
);
