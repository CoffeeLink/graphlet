using System.Collections.Concurrent;

namespace Grahplet.WebSockets;

/// <summary>
/// Background service that manages all LiveSession instances.
/// Acts as an index to look up LiveSessions by WorkspaceId.
/// </summary>
public class SessionDictionary : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, SessionEntry> _sessions = new();
    private readonly ILogger<SessionDictionary> _logger;

    public SessionDictionary(ILogger<SessionDictionary> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets an existing LiveSession or creates a new one for the workspace.
    /// </summary>
    public LiveSession GetOrCreateSession(Guid workspaceId)
    {
        var entry = _sessions.GetOrAdd(workspaceId, id =>
        {
            _logger.LogInformation("Creating new LiveSession for workspace {WorkspaceId}", id);

            var session = new LiveSession(id);
            var cts = new CancellationTokenSource();

            var task = Task.Run(async () =>
            {
                try
                {
                    await session.RunAsync(cts.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LiveSession for workspace {WorkspaceId} encountered an error", id);
                }
            }, cts.Token);

            return new SessionEntry(session, cts, task);
        });

        return entry.Session;
    }

    /// <summary>
    /// Removes a LiveSession for a workspace.
    /// </summary>
    public async Task RemoveSessionAsync(Guid workspaceId)
    {
        if (_sessions.TryRemove(workspaceId, out var entry))
        {
            _logger.LogInformation("Removing LiveSession for workspace {WorkspaceId}", workspaceId);

            entry.Cts.Cancel();

            try
            {
                await entry.Task;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping LiveSession for workspace {WorkspaceId}", workspaceId);
            }
            finally
            {
                entry.Cts.Dispose();
            }
        }
    }

    public int ActiveSessionCount => _sessions.Count;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SessionDictionary started");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SessionDictionary is shutting down");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping all LiveSessions...");

        var stopTasks = _sessions.Keys.Select(RemoveSessionAsync).ToList();
        await Task.WhenAll(stopTasks);

        await base.StopAsync(cancellationToken);
    }

    private record SessionEntry(
        LiveSession Session,
        CancellationTokenSource Cts,
        Task Task
    );
}
