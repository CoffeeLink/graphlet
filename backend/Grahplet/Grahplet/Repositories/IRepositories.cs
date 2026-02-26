using Grahplet.Models;

namespace Grahplet.Repositories;

public interface IAuthRepository
{
    Task<(bool Success, string Token)> LoginAsync(string email, string password);
    Task LogoutAsync(string token);
    Task<bool> ValidateTokenAsync(string token);
    Task<Guid?> GetUserIdFromTokenAsync(string token);
    
    // Identity stuff
    
    Task<User?> CreateUserAsync(string uname, string email, string password);
    Task<User?> GetUserAsync(Guid userId);
    Task<User?> UpdateUserAsync(Guid userId, User user);
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
    Task<List<Note>> GetNotesAsync(Guid userId, Guid workspaceId);
    Task<Note?> GetNoteAsync(Guid userId, Guid workspaceId, Guid noteId);
    Task<Note> CreateNoteAsync(Guid userId, Guid workspaceId, NoteCreate note);
    Task<Note?> UpdateNoteAsync(Guid userId, Guid workspaceId, Guid noteId, NoteUpdate note);
    Task<bool> DeleteNoteAsync(Guid userId, Guid workspaceId, Guid noteId);
    Task<Note?> AttachTagToNoteAsync(Guid userId, Guid workspaceId, Guid noteId, Guid tagId);
    Task<bool> DetachTagFromNoteAsync(Guid userId, Guid workspaceId, Guid noteId, Guid tagId);
    // Relations are managed via the same repository as notes
    Task<NoteRelation?> GetRelationAsync(Guid userId, Guid workspaceId, Guid noteId, Guid relationId);
    Task<NoteRelation> CreateRelationAsync(Guid userId, Guid workspaceId, Guid noteId, NoteRelationCreate relation);
    Task<NoteRelation?> UpdateRelationAsync(Guid userId, Guid workspaceId, Guid noteId, Guid relationId, NoteRelationUpdate relation);
    Task<bool> DeleteRelationAsync(Guid userId, Guid workspaceId, Guid noteId, Guid relationId);
}


public interface IAccessRepository
{
    // Workspace Access
    Task<WorkspaceAccess?> GetWorkspaceAccessAsync(Guid userId, Guid workspaceId);
    Task<List<WorkspaceAccess>> GetWorkspaceAccessListAsync(Guid workspaceId);
    Task<List<Guid>> GetUserWorkspaceIdsAsync(Guid userId);
    Task<WorkspaceAccess> GrantWorkspaceAccessAsync(Guid userId, Guid workspaceId, string accessLevel, Guid? invitedBy = null);
    Task<bool> UpdateWorkspaceAccessAsync(Guid userId, Guid workspaceId, string accessLevel);
    Task<bool> RevokeWorkspaceAccessAsync(Guid userId, Guid workspaceId);
    Task<bool> HasWorkspaceAccessAsync(Guid userId, Guid workspaceId, string? minimumLevel = null);

    // Organization Access
    Task<OrgAccess?> GetOrgAccessAsync(Guid userId, Guid orgId);
    Task<List<OrgAccess>> GetOrgAccessListAsync(Guid orgId);
    Task<List<Guid>> GetUserOrgIdsAsync(Guid userId);
    Task<OrgAccess> GrantOrgAccessAsync(Guid userId, Guid orgId, string accessLevel, Guid? invitedBy = null);
    Task<bool> UpdateOrgAccessAsync(Guid userId, Guid orgId, string accessLevel);
    Task<bool> RevokeOrgAccessAsync(Guid userId, Guid orgId);
    Task<bool> HasOrgAccessAsync(Guid userId, Guid orgId, string? minimumLevel = null);

    // Workspace Invitations
    Task<WorkspaceInvitation?> GetWorkspaceInvitationAsync(Guid invitationId);
    Task<List<WorkspaceInvitation>> GetUserWorkspaceInvitationsAsync(Guid userId);
    Task<WorkspaceInvitation> CreateWorkspaceInvitationAsync(Guid workspaceId, Guid targetUserId, string accessLevel, Guid inviteMadeBy, DateTime? expires = null);
    Task<bool> AcceptWorkspaceInvitationAsync(Guid invitationId, Guid userId);
    Task<bool> DeclineWorkspaceInvitationAsync(Guid invitationId, Guid userId);

    // Org-Workspace ownership
    Task<bool> SetOrgWorkspaceOwnerAsync(Guid orgId, Guid workspaceId);
    Task<bool> RemoveOrgWorkspaceOwnerAsync(Guid orgId, Guid workspaceId);
    Task<Guid?> GetWorkspaceOrgIdAsync(Guid workspaceId);
    Task<List<Guid>> GetOrgWorkspaceIdsAsync(Guid orgId);
}

public interface IOrganizationRepository
{
    Task<List<Organization>> GetUserOrganizationsAsync(Guid userId);
    Task<Organization?> GetOrganizationAsync(Guid userId, Guid orgId);
    Task<Organization> CreateOrganizationAsync(Guid userId, OrganizationCreate org);
    Task<Organization?> UpdateOrganizationAsync(Guid userId, Guid orgId, OrganizationUpdate org);
    Task<bool> DeleteOrganizationAsync(Guid userId, Guid orgId);
}

