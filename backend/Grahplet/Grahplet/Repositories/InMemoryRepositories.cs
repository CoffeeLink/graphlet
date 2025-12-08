using Grahplet.Models;

namespace Grahplet.Repositories;

public class InMemoryAuthRepository : IAuthRepository
{
    private readonly Dictionary<string, Guid> _tokenToUserId = new();
    private readonly Dictionary<Guid, User> _users = new();

    public InMemoryAuthRepository()
    {
        // Create a demo user
        var demoUserId = Guid.NewGuid();
        _users[demoUserId] = new User
        {
            Id = demoUserId,
            Username = "demo",
            Email = "demo@example.com",
            Password = "demo",
            ProfilePicUrl = "https://example.com/pic.jpg",
            LastSeen = DateTime.UtcNow
        };
    }

    public Task<(bool Success, string Token)> LoginAsync(string email, string password)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email == email && u.Password == password);
        if (user == null)
        {
            return Task.FromResult((false, ""));
        }

        var token = Guid.NewGuid().ToString();
        _tokenToUserId[token] = user.Id;
        return Task.FromResult((true, token));
    }

    public Task LogoutAsync(string token)
    {
        _tokenToUserId.Remove(token);
        return Task.CompletedTask;
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        return Task.FromResult(_tokenToUserId.ContainsKey(token));
    }

    public Task<Guid?> GetUserIdFromTokenAsync(string token)
    {
        _tokenToUserId.TryGetValue(token, out var userId);
        return Task.FromResult(userId == Guid.Empty ? null : (Guid?)userId);
    }
}

public class InMemoryWorkspaceRepository : IWorkspaceRepository
{
    private readonly Dictionary<Guid, Workspace> _workspaces = new();
    private readonly Dictionary<Guid, List<Guid>> _userWorkspaces = new(); // userId -> workspaceIds

    public Task<List<Workspace>> GetWorkspacesAsync(Guid userId)
    {
        if (!_userWorkspaces.TryGetValue(userId, out var workspaceIds))
        {
            return Task.FromResult(new List<Workspace>());
        }

        var workspaces = workspaceIds
            .Where(id => _workspaces.ContainsKey(id))
            .Select(id => _workspaces[id])
            .ToList();

        return Task.FromResult(workspaces);
    }

    public Task<Workspace?> GetWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        if (!_userWorkspaces.TryGetValue(userId, out var workspaceIds) || !workspaceIds.Contains(workspaceId))
        {
            return Task.FromResult<Workspace?>(null);
        }

        _workspaces.TryGetValue(workspaceId, out var workspace);
        return Task.FromResult(workspace);
    }

    public Task<Workspace> CreateWorkspaceAsync(Guid userId, WorkspaceCreate workspace)
    {
        var id = Guid.NewGuid();
        var newWorkspace = new Workspace { Id = id, Name = workspace.Name };
        _workspaces[id] = newWorkspace;

        if (!_userWorkspaces.ContainsKey(userId))
        {
            _userWorkspaces[userId] = new List<Guid>();
        }

        _userWorkspaces[userId].Add(id);
        return Task.FromResult(newWorkspace);
    }

    public Task<Workspace?> UpdateWorkspaceAsync(Guid userId, Guid workspaceId, WorkspaceUpdate workspace)
    {
        var existing = GetWorkspaceAsync(userId, workspaceId).Result;
        if (existing == null)
        {
            return Task.FromResult<Workspace?>(null);
        }

        if (!string.IsNullOrEmpty(workspace.Name))
        {
            existing.Name = workspace.Name;
        }

        return Task.FromResult<Workspace?>(existing);
    }

    public Task<bool> DeleteWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        if (!_userWorkspaces.TryGetValue(userId, out var workspaceIds) || !workspaceIds.Contains(workspaceId))
        {
            return Task.FromResult(false);
        }

        workspaceIds.Remove(workspaceId);
        _workspaces.Remove(workspaceId);
        return Task.FromResult(true);
    }
}

public class InMemoryTagRepository : ITagRepository
{
    private readonly Dictionary<Guid, Tag> _tags = new();
    private readonly Dictionary<(Guid UserId, Guid WorkspaceId), List<Guid>> _workspaceTags = new();

    public Task<Tag?> GetTagAsync(Guid userId, Guid workspaceId, Guid tagId)
    {
        var key = (userId, workspaceId);
        if (!_workspaceTags.TryGetValue(key, out var tagIds) || !tagIds.Contains(tagId))
        {
            return Task.FromResult<Tag?>(null);
        }

        _tags.TryGetValue(tagId, out var tag);
        return Task.FromResult(tag);
    }

    public Task<List<Tag>> GetTagsAsync(Guid userId, Guid workspaceId)
    {
        var key = (userId, workspaceId);
        if (!_workspaceTags.TryGetValue(key, out var tagIds))
        {
            return Task.FromResult(new List<Tag>());
        }

        var tags = tagIds
            .Where(id => _tags.ContainsKey(id))
            .Select(id => _tags[id])
            .ToList();

        return Task.FromResult(tags);
    }

    public Task<Tag> CreateTagAsync(Guid userId, Guid workspaceId, TagCreate tag)
    {
        var id = Guid.NewGuid();
        var newTag = new Tag { Id = id, Name = tag.Name, Color = tag.Color };
        _tags[id] = newTag;

        var key = (userId, workspaceId);
        if (!_workspaceTags.ContainsKey(key))
        {
            _workspaceTags[key] = new List<Guid>();
        }

        _workspaceTags[key].Add(id);
        return Task.FromResult(newTag);
    }

    public Task<Tag?> UpdateTagAsync(Guid userId, Guid workspaceId, Guid tagId, TagUpdate tag)
    {
        var existing = GetTagAsync(userId, workspaceId, tagId).Result;
        if (existing == null)
        {
            return Task.FromResult<Tag?>(null);
        }

        if (!string.IsNullOrEmpty(tag.Name))
        {
            existing.Name = tag.Name;
        }

        if (tag.Color != null)
        {
            existing.Color = tag.Color;
        }

        return Task.FromResult<Tag?>(existing);
    }

    public Task<bool> DeleteTagAsync(Guid userId, Guid workspaceId, Guid tagId)
    {
        var key = (userId, workspaceId);
        if (!_workspaceTags.TryGetValue(key, out var tagIds) || !tagIds.Contains(tagId))
        {
            return Task.FromResult(false);
        }

        tagIds.Remove(tagId);
        _tags.Remove(tagId);
        return Task.FromResult(true);
    }
}

public class InMemoryNoteRepository : INoteRepository
{
    private readonly Dictionary<Guid, Note> _notes = new();
    private readonly Dictionary<Guid, List<Guid>> _userNotes = new(); // userId -> noteIds
    private readonly Dictionary<(Guid NoteId, Guid TagId), bool> _noteTags = new();

    public Task<List<Note>> GetNotesAsync(Guid userId)
    {
        if (!_userNotes.TryGetValue(userId, out var noteIds))
        {
            return Task.FromResult(new List<Note>());
        }

        var notes = noteIds
            .Where(id => _notes.ContainsKey(id))
            .Select(id => _notes[id])
            .ToList();

        return Task.FromResult(notes);
    }

    public Task<Note?> GetNoteAsync(Guid userId, Guid noteId)
    {
        if (!_userNotes.TryGetValue(userId, out var noteIds) || !noteIds.Contains(noteId))
        {
            return Task.FromResult<Note?>(null);
        }

        _notes.TryGetValue(noteId, out var note);
        return Task.FromResult(note);
    }

    public Task<Note> CreateNoteAsync(Guid userId, NoteCreate note)
    {
        var id = Guid.NewGuid();
        var newNote = new Note
        {
            Id = id,
            Name = note.Name,
            Kind = note.Kind,
            Value = note.Value,
            File = note.File,
            Tags = new List<Tag>(),
            Relations = new List<NoteRelation>()
        };

        _notes[id] = newNote;

        if (!_userNotes.ContainsKey(userId))
        {
            _userNotes[userId] = new List<Guid>();
        }

        _userNotes[userId].Add(id);
        return Task.FromResult(newNote);
    }

    public Task<Note?> UpdateNoteAsync(Guid userId, Guid noteId, NoteUpdate note)
    {
        var existing = GetNoteAsync(userId, noteId).Result;
        if (existing == null)
        {
            return Task.FromResult<Note?>(null);
        }

        if (!string.IsNullOrEmpty(note.Name))
        {
            existing.Name = note.Name;
        }

        if (!string.IsNullOrEmpty(note.Kind))
        {
            existing.Kind = note.Kind;
        }

        if (note.Value != null)
        {
            existing.Value = note.Value;
        }

        if (note.File != null)
        {
            existing.File = note.File;
        }

        return Task.FromResult<Note?>(existing);
    }

    public Task<bool> DeleteNoteAsync(Guid userId, Guid noteId)
    {
        if (!_userNotes.TryGetValue(userId, out var noteIds) || !noteIds.Contains(noteId))
        {
            return Task.FromResult(false);
        }

        noteIds.Remove(noteId);
        _notes.Remove(noteId);
        return Task.FromResult(true);
    }

    public Task<Note?> AttachTagToNoteAsync(Guid userId, Guid noteId, Guid tagId)
    {
        var note = GetNoteAsync(userId, noteId).Result;
        if (note == null)
        {
            return Task.FromResult<Note?>(null);
        }

        // Note: In a real implementation, we would verify the tag exists
        _noteTags[(noteId, tagId)] = true;

        // This is simplified - in real implementation, we'd fetch the actual tag
        if (!note.Tags.Any(t => t.Id == tagId))
        {
            note.Tags.Add(new Tag { Id = tagId, Name = "", Color = new int[3] });
        }

        return Task.FromResult<Note?>(note);
    }

    public Task<bool> DetachTagFromNoteAsync(Guid userId, Guid noteId, Guid tagId)
    {
        var note = GetNoteAsync(userId, noteId).Result;
        if (note == null)
        {
            return Task.FromResult(false);
        }

        var removed = _noteTags.Remove((noteId, tagId));
        note.Tags.RemoveAll(t => t.Id == tagId);
        return Task.FromResult(removed || note.Tags.Count > 0);
    }
}

public class InMemoryNoteRelationRepository : INoteRelationRepository
{
    private readonly Dictionary<Guid, NoteRelation> _relations = new();
    private readonly Dictionary<Guid, List<Guid>> _noteRelations = new(); // noteId -> relationIds

    public Task<NoteRelation?> GetRelationAsync(Guid userId, Guid noteId, Guid relationId)
    {
        if (!_noteRelations.TryGetValue(noteId, out var relationIds) || !relationIds.Contains(relationId))
        {
            return Task.FromResult<NoteRelation?>(null);
        }

        _relations.TryGetValue(relationId, out var relation);
        return Task.FromResult(relation);
    }

    public Task<NoteRelation> CreateRelationAsync(Guid userId, Guid noteId, NoteRelationCreate relation)
    {
        var id = Guid.NewGuid();
        var newRelation = new NoteRelation
        {
            Id = id,
            Connection = new[] { noteId, relation.OtherId },
            Name = relation.Name
        };

        _relations[id] = newRelation;

        if (!_noteRelations.ContainsKey(noteId))
        {
            _noteRelations[noteId] = new List<Guid>();
        }

        _noteRelations[noteId].Add(id);
        return Task.FromResult(newRelation);
    }

    public Task<NoteRelation?> UpdateRelationAsync(Guid userId, Guid noteId, Guid relationId, NoteRelationUpdate relation)
    {
        var existing = GetRelationAsync(userId, noteId, relationId).Result;
        if (existing == null)
        {
            return Task.FromResult<NoteRelation?>(null);
        }

        if (!string.IsNullOrEmpty(relation.Name))
        {
            existing.Name = relation.Name;
        }

        return Task.FromResult<NoteRelation?>(existing);
    }

    public Task<bool> DeleteRelationAsync(Guid userId, Guid noteId, Guid relationId)
    {
        if (!_noteRelations.TryGetValue(noteId, out var relationIds) || !relationIds.Contains(relationId))
        {
            return Task.FromResult(false);
        }

        relationIds.Remove(relationId);
        _relations.Remove(relationId);
        return Task.FromResult(true);
    }
}

