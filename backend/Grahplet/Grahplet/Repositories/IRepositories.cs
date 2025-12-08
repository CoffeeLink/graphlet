using Grahplet.Models;

namespace Grahplet.Repositories;

public interface IAuthRepository
{
    Task<(bool Success, string Token)> LoginAsync(string email, string password);
    Task LogoutAsync(string token);
    Task<bool> ValidateTokenAsync(string token);
    Task<Guid?> GetUserIdFromTokenAsync(string token);
}

public interface IWorkspaceRepository
{
    Task<List<Workspace>> GetWorkspacesAsync(Guid userId);
    Task<Workspace?> GetWorkspaceAsync(Guid userId, Guid workspaceId);
    Task<Workspace> CreateWorkspaceAsync(Guid userId, WorkspaceCreate workspace);
    Task<Workspace?> UpdateWorkspaceAsync(Guid userId, Guid workspaceId, WorkspaceUpdate workspace);
    Task<bool> DeleteWorkspaceAsync(Guid userId, Guid workspaceId);
}

public interface ITagRepository
{
    Task<Tag?> GetTagAsync(Guid userId, Guid workspaceId, Guid tagId);
    Task<List<Tag>> GetTagsAsync(Guid userId, Guid workspaceId);
    Task<Tag> CreateTagAsync(Guid userId, Guid workspaceId, TagCreate tag);
    Task<Tag?> UpdateTagAsync(Guid userId, Guid workspaceId, Guid tagId, TagUpdate tag);
    Task<bool> DeleteTagAsync(Guid userId, Guid workspaceId, Guid tagId);
}

public interface INoteRepository
{
    Task<List<Note>> GetNotesAsync(Guid userId);
    Task<Note?> GetNoteAsync(Guid userId, Guid noteId);
    Task<Note> CreateNoteAsync(Guid userId, NoteCreate note);
    Task<Note?> UpdateNoteAsync(Guid userId, Guid noteId, NoteUpdate note);
    Task<bool> DeleteNoteAsync(Guid userId, Guid noteId);
    Task<Note?> AttachTagToNoteAsync(Guid userId, Guid noteId, Guid tagId);
    Task<bool> DetachTagFromNoteAsync(Guid userId, Guid noteId, Guid tagId);
}

public interface INoteRelationRepository
{
    Task<NoteRelation?> GetRelationAsync(Guid userId, Guid noteId, Guid relationId);
    Task<NoteRelation> CreateRelationAsync(Guid userId, Guid noteId, NoteRelationCreate relation);
    Task<NoteRelation?> UpdateRelationAsync(Guid userId, Guid noteId, Guid relationId, NoteRelationUpdate relation);
    Task<bool> DeleteRelationAsync(Guid userId, Guid noteId, Guid relationId);
}

