# WebSocket Architecture

## Overview
This document describes the clean WebSocket architecture for real-time collaborative sessions.

## Architecture Components

### 1. SessionClient (Transient)
**Location:** `Grahplet/WebSockets/SessionClient.cs`

Each WebSocket connection gets its own SessionClient instance that handles:
- **Lifecycle Management**: Connect, authenticate, messaging, and close
- **Periodic Pinging**: Sends pings every 5 seconds to verify client is still connected
- **Action Relaying**: Forwards ClientAction messages from the client to the LiveSession

**Connection Flow:**
1. Client connects and sends Auth message with token and workspaceId
2. SessionClient validates token and workspace access
3. Connects to the appropriate LiveSession via channels (Tx/Rx)
4. Receives SessionInfo from LiveSession and sends to client
5. Runs message loop handling client messages and LiveSession broadcasts

### 2. LiveSession (Task per Workspace)
**Location:** `Grahplet/WebSockets/LiveSession.cs`

Each instance is tied to an active workspace. Responsibilities:
- **Client Management**: Tracks connected clients and kicks off old connections when same user connects twice
- **Lock Management**: Maintains a dictionary of locks (NoteId -> UserId), clients can lock only one thing at a time
- **Action Broadcasting**: Relays ClientActions to all other connected clients

**Key Features:**
- Automatically removes old connections when a user connects from a new session
- Validates lock/unlock actions (only lock holder can unlock)
- Broadcasts UserJoined/UserLeft events to all clients

### 3. SessionDictionary (Background Service)
**Location:** `Grahplet/WebSockets/SessionDictionary.cs`

A singleton background service that:
- **Session Index**: Maintains a dictionary of WorkspaceId -> LiveSession
- **Task Management**: Manages LiveSession tasks and their cancellation tokens
- **Lifecycle**: Creates sessions on-demand, cleans up on shutdown

**Key Methods:**
- `GetOrCreateSession(Guid workspaceId)` - Returns existing or creates new LiveSession
- `RemoveSessionAsync(Guid workspaceId)` - Stops and removes a LiveSession
- `ActiveSessionCount` - Returns number of active sessions

## Message Types

All messages use JSON polymorphic serialization with a `type` discriminator field.

### System Messages (Server ↔ Client)
System-level messages for connection management:
- **Ping** - `{ "type": "Ping" }` - Server sends to check if client is alive
- **Pong** - `{ "type": "Pong" }` - Client responds to Ping
- **Auth** - `{ "type": "Auth", "token": "...", "workspace": "..." }` - Client sends to authenticate
- **Disconnect** - `{ "type": "Disconnect" }` - Client sends to gracefully disconnect
- **SessionInfo** - `{ "type": "SessionInfo", "users": [...], "locks": {...} }` - Server sends current session state
- **MessageAck** - `{ "type": "MessageAck", "messageId": "..." }` - Future: acknowledgment for reliable delivery

### Session Events (Server → Client)
Session-level events:
- **UserJoined** - `{ "type": "UserJoined", "userId": "..." }` - A user joined the workspace
- **UserLeft** - `{ "type": "UserLeft", "userId": "..." }` - A user left the workspace
- **ServerError** - `{ "type": "ServerError", "message": "..." }` - Error message from server

### Client Actions (Client ↔ Server)
Actions performed by clients:
- **Lock** - `{ "type": "Lock", "userId": "...", "noteId": "..." }` - Lock a note for editing
- **Unlock** - `{ "type": "Unlock", "userId": "...", "noteId": "..." }` - Unlock a note
- **Custom** - `{ "type": "Custom", "userId": "...", "customType": "...", "payload": {...} }` - Custom client-defined action

All actions include the userId so other clients know who performed the action.

## Internal Messages (Server-Side Only)

These messages are used for communication between SessionClient and LiveSession:
- **ClientConnectInternal** - SessionClient notifies LiveSession of new connection
- **ClientDisconnectInternal** - SessionClient notifies LiveSession of disconnection
- **ClientActionInternal** - SessionClient forwards client action to LiveSession

## Connection Flow

```
1. Client connects to /ws/live
2. Client sends Auth { token, workspaceId }
3. Server validates token and workspace access
4. SessionClient connects to LiveSession via SessionDictionary
5. LiveSession checks for existing connection from same user
   - If exists: kicks off old connection
   - Adds new connection
6. LiveSession sends SessionInfo { users, locks } to new client
7. LiveSession broadcasts UserJoined to all other clients
8. Client can now send/receive messages
9. On disconnect: LiveSession broadcasts UserLeft and removes client
```

## Key Design Decisions

### ConnectionId vs UserId
- **ConnectionId**: Server-side only, unique per WebSocket connection
- **UserId**: Shared with clients, identifies the user
- This allows the server to differentiate between old and new connections from the same user

### Single Connection Per User
When a user connects from a new session, the old connection is automatically kicked off. This prevents:
- Duplicate messages
- Stale connections
- Lock conflicts

### Lock Management
- Each user can lock only one note at a time
- Only the lock holder can unlock
- Locks are automatically released when user disconnects
- Lock attempts on already-locked notes return ServerError

### Channel-Based Communication
- SessionClient → LiveSession: via `LiveSession.Rx` channel
- LiveSession → SessionClient: via per-client `ClientTx` channel
- This provides thread-safe, async communication without locks

## Dependency Injection

```csharp
// Register SessionDictionary as a singleton hosted service
builder.Services.AddSingleton<SessionDictionary>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SessionDictionary>());

// SessionClient should be transient (one per WebSocket connection)
builder.Services.AddTransient<SessionClient>();
```

## Testing

See `WebSocket.http` for test cases:
- **WS-001**: Auth + SessionInfo + Ping/Pong
- **WS-002**: Lock and Unlock Actions
- **WS-003**: Custom Client Action

## Benefits

1. **Simple Architecture**: Only 3 moving parts with clear responsibilities
2. **Scalable**: Sessions created on-demand per workspace
3. **Thread-Safe**: Channel-based communication
4. **Extensible**: Easy to add new message types via CustomClientAction
5. **Robust**: Automatic cleanup of stale connections and locks
