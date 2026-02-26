using Grahplet.Data;
using Grahplet.Models;
using Grahplet.Security;
using Microsoft.EntityFrameworkCore;
using MFileInfo = Grahplet.Models.FileInfo;

namespace Grahplet.Repositories;

internal static class AccessLevelHelper
{
    public static bool MeetsMinimum(AccessLevel current, string? minimum)
    {
        if (string.IsNullOrEmpty(minimum)) return true;

        var levels = new[] { "Read", "Write", "Admin", "Owner" };
        var currentIndex = Array.IndexOf(levels, current.ToString());
        var minimumIndex = Array.IndexOf(levels, minimum);

        return currentIndex >= minimumIndex;
    }

    public static AccessLevel Parse(string level)
    {
        return level switch
        {
            "Read" => AccessLevel.Read,
            "Write" => AccessLevel.Write,
            "Admin" => AccessLevel.Admin,
            "Owner" => AccessLevel.Owner,
            _ => AccessLevel.Read
        };
    }
}

internal static class EfMap
{
    public static Workspace ToModel(this DbWorkspace e) => new Workspace { Id = e.Id, Name = e.Name };
    public static User ToModel(this DbUser e) => new User 
    { 
        Id = e.Id, 
        Username = e.Username, 
        Email = e.Email, 
        ProfilePicUrl = e.ProfilePicUrl, 
        LastSeen = e.LastSeen 
    };
    public static Tag ToModel(this DbTag e)
    {
        var parts = (e.Color ?? "0,0,0").Split(',');
        var rgb = new int[3];
        for (var i = 0; i < 3 && i < parts.Length; i++) int.TryParse(parts[i], out rgb[i]);
        return new Tag { Id = e.Id, Name = e.Name, Color = rgb };
    }
    public static Note ToModel(this DbNote n, List<Tag> tags, List<NoteRelation> relations)
    {
        return new Note
        {
            Id = n.Id,
            Name = n.Name,
            Kind = n.Kind,
            Value = n.Value,
            File = (n.File_Id.HasValue || !string.IsNullOrEmpty(n.File_Filename))
                ? new MFileInfo
                {
                    Id = n.File_Id ?? Guid.Empty,
                    Filename = n.File_Filename ?? string.Empty,
                    ContentType = n.File_ContentType ?? string.Empty,
                    ContentUrl = n.File_ContentUrl ?? string.Empty
                }
                : null,
            WorkspaceId = n.WorkspaceId,
            PositionX = n.PositionX,
            PositionY = n.PositionY,
            Tags = tags,
            Relations = relations
        };
    }

    public static DbNote Apply(this DbNote e, NoteCreate m, Guid workspaceId)
    {
        e.Id = e.Id == Guid.Empty ? Guid.NewGuid() : e.Id;
        e.Name = m.Name;
        e.Kind = m.Kind;
        e.Value = m.Value;
        if (m.File != null)
        {
            e.File_Id = m.File.Id == Guid.Empty ? Guid.NewGuid() : m.File.Id;
            e.File_Filename = m.File.Filename;
            e.File_ContentType = m.File.ContentType;
            e.File_ContentUrl = m.File.ContentUrl;
        }
        e.WorkspaceId = workspaceId;
        e.PositionX = m.PositionX;
        e.PositionY = m.PositionY;
        return e;
    }

    public static void Apply(this DbNote e, NoteUpdate m)
    {
        if (!string.IsNullOrWhiteSpace(m.Name)) e.Name = m.Name!;
        if (!string.IsNullOrWhiteSpace(m.Kind)) e.Kind = m.Kind!;
        if (m.Value != null) e.Value = m.Value;
        if (m.File != null)
        {
            e.File_Id = m.File.Id == Guid.Empty ? (e.File_Id ?? Guid.NewGuid()) : m.File.Id;
            e.File_Filename = m.File.Filename;
            e.File_ContentType = m.File.ContentType;
            e.File_ContentUrl = m.File.ContentUrl;
        }
        if (m.PositionX.HasValue) e.PositionX = m.PositionX.Value;
        if (m.PositionY.HasValue) e.PositionY = m.PositionY.Value;
    }
}

public class EfAuthRepository(GrahpletDbContext db) : IAuthRepository
{
    public async Task<(bool Success, string Token)> LoginAsync(string email, string password)
    {
        // Fetch by email only, then verify password against stored hash
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return (false, "");

        if (!PasswordHasher.Verify(user.PasswordHash, password))
            return (false, "");
        var token = Guid.NewGuid().ToString();
        db.SessionTokens.Add(new DbSessionToken { Token = token, UserId = user.Id });
        await db.SaveChangesAsync();
        return (true, token);
    }

    public async Task LogoutAsync(string token)
    {
        var entity = await db.SessionTokens.FindAsync(token);
        if (entity != null)
        {
            db.SessionTokens.Remove(entity);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        return await db.SessionTokens.AnyAsync(t => t.Token == token);
    }

    public async Task<Guid?> GetUserIdFromTokenAsync(string token)
    {
        var t = await db.SessionTokens.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token);
        return t?.UserId;
    }

    public async Task<User?> CreateUserAsync(string uname, string email, string password)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == email || u.Username == uname);
        if (exists) return null;

        var user = new DbUser
        {
            Id = Guid.NewGuid(),
            Username = uname,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            ProfilePicUrl = string.Empty,
            LastSeen = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.ToModel();
    }

    public async Task<User?> GetUserAsync(Guid userId)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        return user?.ToModel();
    }

    public async Task<User?> UpdateUserAsync(Guid userId, User user)
    {
        var e = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (e == null) return null;
        if (!string.IsNullOrWhiteSpace(user.Username)) e.Username = user.Username;
        if (!string.IsNullOrWhiteSpace(user.Email)) e.Email = user.Email;
        if (!string.IsNullOrWhiteSpace(user.ProfilePicUrl)) e.ProfilePicUrl = user.ProfilePicUrl;
        e.LastSeen = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return e.ToModel();
    }
}

public class EfWorkspaceRepository(GrahpletDbContext db) : IWorkspaceRepository
{
    public async Task<List<Workspace>> GetWorkspacesAsync(Guid userId)
    {
        // Get workspace IDs user has access to
        var workspaceIds = await db.UserWorkspaceAccess
            .Where(a => a.UserId == userId)
            .Select(a => a.WorkspaceId)
            .ToListAsync();

        return await db.Workspaces
            .Where(w => workspaceIds.Contains(w.Id))
            .Select(w => w.ToModel())
            .ToListAsync();
    }

    public async Task<Workspace?> GetWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        // No auth check - controller handles this
        var e = await db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workspaceId);
        return e?.ToModel();
    }

    public async Task<Workspace> CreateWorkspaceAsync(Guid userId, WorkspaceCreate workspace)
    {
        var e = new DbWorkspace { Id = Guid.NewGuid(), Name = workspace.Name };
        db.Workspaces.Add(e);

        // Grant owner access to creator
        db.UserWorkspaceAccess.Add(new DbUserWorkspaceAccess
        {
            UserId = userId,
            WorkspaceId = e.Id,
            AccessLevel = AccessLevel.Owner,
            InvitedBy = null
        });

        await db.SaveChangesAsync();
        return e.ToModel();
    }

    public async Task<Workspace?> UpdateWorkspaceAsync(Guid userId, Guid workspaceId, WorkspaceUpdate workspace)
    {
        // No auth check - controller handles this
        var e = await db.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (e == null) return null;
        if (!string.IsNullOrWhiteSpace(workspace.Name)) e.Name = workspace.Name!;
        await db.SaveChangesAsync();
        return e.ToModel();
    }

    public async Task<bool> DeleteWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        // No auth check - controller handles this
        var e = await db.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (e == null) return false;

        // Remove all access entries
        var accessEntries = db.UserWorkspaceAccess.Where(a => a.WorkspaceId == workspaceId);
        db.UserWorkspaceAccess.RemoveRange(accessEntries);

        // Remove org ownership if any
        var orgOwnership = db.OrgWorkspaceOwners.Where(o => o.WorkspaceId == workspaceId);
        db.OrgWorkspaceOwners.RemoveRange(orgOwnership);

        db.Workspaces.Remove(e);
        await db.SaveChangesAsync();
        return true;
    }
}

public class EfTagRepository(GrahpletDbContext db) : ITagRepository
{
    private static string ColorToString(int[] color) => string.Join(',', color ?? new[] { 0, 0, 0 });

    public async Task<Tag?> GetTagAsync(Guid userId, Guid workspaceId, Guid tagId)
    {
        // No auth check - controller handles this
        var e = await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tagId && t.WorkspaceId == workspaceId);
        return e?.ToModel();
    }

    public async Task<List<Tag>> GetTagsAsync(Guid userId, Guid workspaceId)
    {
        // No auth check - controller handles this
        return await db.Tags.Where(t => t.WorkspaceId == workspaceId)
            .AsNoTracking().Select(t => t.ToModel()).ToListAsync();
    }

    public async Task<Tag> CreateTagAsync(Guid userId, Guid workspaceId, TagCreate tag)
    {
        // No auth check - controller handles this
        var e = new DbTag
        {
            Id = Guid.NewGuid(),
            Name = tag.Name,
            Color = ColorToString(tag.Color),
            WorkspaceId = workspaceId
        };
        db.Tags.Add(e);
        await db.SaveChangesAsync();
        return e.ToModel();
    }

    public async Task<Tag?> UpdateTagAsync(Guid userId, Guid workspaceId, Guid tagId, TagUpdate tag)
    {
        // No auth check - controller handles this
        var e = await db.Tags.FirstOrDefaultAsync(t => t.Id == tagId && t.WorkspaceId == workspaceId);
        if (e == null) return null;
        if (!string.IsNullOrWhiteSpace(tag.Name)) e.Name = tag.Name!;
        if (tag.Color != null) e.Color = ColorToString(tag.Color);
        await db.SaveChangesAsync();
        return e.ToModel();
    }

    public async Task<bool> DeleteTagAsync(Guid userId, Guid workspaceId, Guid tagId)
    {
        // No auth check - controller handles this
        var e = await db.Tags.FirstOrDefaultAsync(t => t.Id == tagId && t.WorkspaceId == workspaceId);
        if (e == null) return false;
        db.Tags.Remove(e);
        await db.SaveChangesAsync();
        return true;
    }
}

public class EfNoteRepository(GrahpletDbContext db) : INoteRepository
{
    public async Task<List<Note>> GetNotesAsync(Guid userId, Guid workspaceId)
    {
        // No auth check - controller handles this
        var notes = await db.Notes.AsNoTracking().Where(n => n.WorkspaceId == workspaceId).ToListAsync();
        var noteIds = notes.Select(n => n.Id).ToList();
        var noteTags = await db.NoteTags.AsNoTracking().Where(nt => noteIds.Contains(nt.NoteId)).ToListAsync();
        var tagIds = noteTags.Select(nt => nt.TagId).Distinct().ToList();
        var tags = await db.Tags.AsNoTracking().Where(t => tagIds.Contains(t.Id)).ToListAsync();
        var rels = await db.NoteRelations.AsNoTracking().Where(r => noteIds.Contains(r.Note1Id) || noteIds.Contains(r.Note2Id)).ToListAsync();

        var tagLookup = tags.ToDictionary(t => t.Id, t => t.ToModel());
        var ntLookup = noteTags.GroupBy(x => x.NoteId).ToDictionary(g => g.Key, g => g.Select(x => x.TagId).ToList());
        var relLookup = new Dictionary<Guid, List<NoteRelation>>();
        foreach (var r in rels)
        {
            void add(Guid noteId)
            {
                if (!relLookup.TryGetValue(noteId, out var list)) { list = []; relLookup[noteId] = list; }
                list.Add(new NoteRelation
                {
                    Id = r.Id,
                    Connection = new[] { r.Note1Id, r.Note2Id },
                    Name = r.Name
                });
            }
            add(r.Note1Id);
            add(r.Note2Id);
        }

        return notes.Select(n => n.ToModel(
            tags: ntLookup.TryGetValue(n.Id, out var tids) ? tids.Select(id => tagLookup[id]).ToList() : new List<Tag>(),
            relations: relLookup.TryGetValue(n.Id, out var rs) ? rs : new List<NoteRelation>()
        )).ToList();
    }

    public async Task<Note?> GetNoteAsync(Guid userId, Guid workspaceId, Guid noteId)
    {
        // No auth check - controller handles this
        var n = await db.Notes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == noteId && x.WorkspaceId == workspaceId);
        if (n == null) return null;
        var noteTags = await db.NoteTags.AsNoTracking().Where(nt => nt.NoteId == noteId).ToListAsync();
        var tagIds = noteTags.Select(x => x.TagId).ToList();
        var tags = await db.Tags.AsNoTracking().Where(t => tagIds.Contains(t.Id)).ToListAsync();
        var rels = await db.NoteRelations.AsNoTracking().Where(r => r.Note1Id == noteId || r.Note2Id == noteId).ToListAsync();
        var relModels = rels.Select(r => new NoteRelation { Id = r.Id, Connection = new[] { r.Note1Id, r.Note2Id }, Name = r.Name }).ToList();
        return n.ToModel(tags.Select(t => t.ToModel()).ToList(), relModels);
    }

    public async Task<Note> CreateNoteAsync(Guid userId, Guid workspaceId, NoteCreate note)
    {
        // No auth check - controller handles this
        var e = new DbNote().Apply(note, workspaceId);
        db.Notes.Add(e);
        await db.SaveChangesAsync();

        // Attach initial tags if provided (workspace filtering at note level)
        if (note.Tags != null && note.Tags.Count > 0)
        {
            var validTagIds = await db.Tags.Where(t => t.WorkspaceId == workspaceId && note.Tags.Contains(t.Id)).Select(t => t.Id).ToListAsync();
            foreach (var tagId in validTagIds)
            {
                db.NoteTags.Add(new DbNoteTag { NoteId = e.Id, TagId = tagId });
            }
            await db.SaveChangesAsync();
        }

        var created = await GetNoteAsync(userId, workspaceId, e.Id);
        return created!;
    }

    public async Task<Note?> UpdateNoteAsync(Guid userId, Guid workspaceId, Guid noteId, NoteUpdate note)
    {
        // No auth check - controller handles this
        var e = await db.Notes.FirstOrDefaultAsync(x => x.Id == noteId && x.WorkspaceId == workspaceId);
        if (e == null) return null;
        e.Apply(note);
        await db.SaveChangesAsync();
        return await GetNoteAsync(userId, workspaceId, noteId);
    }

    public async Task<bool> DeleteNoteAsync(Guid userId, Guid workspaceId, Guid noteId)
    {
        // No auth check - controller handles this
        var e = await db.Notes.FirstOrDefaultAsync(x => x.Id == noteId && x.WorkspaceId == workspaceId);
        if (e == null) return false;
        // cascade-like cleanup
        var nts = db.NoteTags.Where(nt => nt.NoteId == noteId);
        db.NoteTags.RemoveRange(nts);
        var rels = db.NoteRelations.Where(r => r.Note1Id == noteId || r.Note2Id == noteId);
        db.NoteRelations.RemoveRange(rels);
        db.Notes.Remove(e);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Note?> AttachTagToNoteAsync(Guid userId, Guid workspaceId, Guid noteId, Guid tagId)
    {
        // No auth check - controller handles this
        var note = await db.Notes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == noteId && n.WorkspaceId == workspaceId);
        if (note == null) return null;
        var tag = await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tagId && t.WorkspaceId == workspaceId);
        if (tag == null) return null;
        var exists = await db.NoteTags.AnyAsync(x => x.NoteId == noteId && x.TagId == tagId);
        if (!exists)
        {
            db.NoteTags.Add(new DbNoteTag { NoteId = noteId, TagId = tagId });
            await db.SaveChangesAsync();
        }
        return await GetNoteAsync(userId, workspaceId, noteId);
    }

    public async Task<bool> DetachTagFromNoteAsync(Guid userId, Guid workspaceId, Guid noteId, Guid tagId)
    {
        // No auth check - controller handles this
        var note = await db.Notes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == noteId && n.WorkspaceId == workspaceId);
        if (note == null) return false;
        var link = await db.NoteTags.FirstOrDefaultAsync(x => x.NoteId == noteId && x.TagId == tagId);
        if (link == null) return false;
        db.NoteTags.Remove(link);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<NoteRelation?> GetRelationAsync(Guid userId, Guid workspaceId, Guid noteId, Guid relationId)
    {
        // No auth check - controller handles this
        // Verify the note belongs to the workspace
        var noteExists = await db.Notes.AsNoTracking().AnyAsync(n => n.Id == noteId && n.WorkspaceId == workspaceId);
        if (!noteExists) return null;

        var r = await db.NoteRelations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == relationId && (x.Note1Id == noteId || x.Note2Id == noteId));
        if (r == null) return null;
        return new NoteRelation { Id = r.Id, Connection = new[] { r.Note1Id, r.Note2Id }, Name = r.Name };
    }

    public async Task<NoteRelation> CreateRelationAsync(Guid userId, Guid workspaceId, Guid noteId, NoteRelationCreate relation)
    {
        // No auth check - controller handles this
        // Ensure both notes belong to the workspace
        var exists1 = await db.Notes.AnyAsync(n => n.Id == noteId && n.WorkspaceId == workspaceId);
        var exists2 = await db.Notes.AnyAsync(n => n.Id == relation.OtherId && n.WorkspaceId == workspaceId);
        if (!exists1 || !exists2) throw new InvalidOperationException("Note not found or access denied");
        var e = new DbNoteRelation { Id = Guid.NewGuid(), Note1Id = noteId, Note2Id = relation.OtherId, Name = relation.Name };
        db.NoteRelations.Add(e);
        await db.SaveChangesAsync();
        return new NoteRelation { Id = e.Id, Connection = new[] { e.Note1Id, e.Note2Id }, Name = e.Name };
    }

    public async Task<NoteRelation?> UpdateRelationAsync(Guid userId, Guid workspaceId, Guid noteId, Guid relationId, NoteRelationUpdate relation)
    {
        // No auth check - controller handles this
        // Verify the note belongs to the workspace
        var noteExists = await db.Notes.AsNoTracking().AnyAsync(n => n.Id == noteId && n.WorkspaceId == workspaceId);
        if (!noteExists) return null;

        var e = await db.NoteRelations.FirstOrDefaultAsync(r => r.Id == relationId && (r.Note1Id == noteId || r.Note2Id == noteId));
        if (e == null) return null;
        if (!string.IsNullOrWhiteSpace(relation.Name)) e.Name = relation.Name!;
        await db.SaveChangesAsync();
        return new NoteRelation { Id = e.Id, Connection = new[] { e.Note1Id, e.Note2Id }, Name = e.Name };
    }

    public async Task<bool> DeleteRelationAsync(Guid userId, Guid workspaceId, Guid noteId, Guid relationId)
    {
        // No auth check - controller handles this
        // Verify the note belongs to the workspace
        var noteExists = await db.Notes.AsNoTracking().AnyAsync(n => n.Id == noteId && n.WorkspaceId == workspaceId);
        if (!noteExists) return false;

        var e = await db.NoteRelations.FirstOrDefaultAsync(r => r.Id == relationId && (r.Note1Id == noteId || r.Note2Id == noteId));
        if (e == null) return false;
        db.NoteRelations.Remove(e);
        await db.SaveChangesAsync();
        return true;
    }
}

public class EfAccessRepository(GrahpletDbContext db) : IAccessRepository
{
    // Workspace Access
    public async Task<WorkspaceAccess?> GetWorkspaceAccessAsync(Guid userId, Guid workspaceId)
    {
        var access = await db.UserWorkspaceAccess.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.WorkspaceId == workspaceId);

        if (access == null) return null;

        return new WorkspaceAccess
        {
            UserId = access.UserId,
            WorkspaceId = access.WorkspaceId,
            AccessLevel = access.AccessLevel.ToString(),
            InvitedBy = access.InvitedBy
        };
    }

    public async Task<List<WorkspaceAccess>> GetWorkspaceAccessListAsync(Guid workspaceId)
    {
        return await db.UserWorkspaceAccess.AsNoTracking()
            .Where(a => a.WorkspaceId == workspaceId)
            .Select(a => new WorkspaceAccess
            {
                UserId = a.UserId,
                WorkspaceId = a.WorkspaceId,
                AccessLevel = a.AccessLevel.ToString(),
                InvitedBy = a.InvitedBy
            })
            .ToListAsync();
    }

    public async Task<List<Guid>> GetUserWorkspaceIdsAsync(Guid userId)
    {
        return await db.UserWorkspaceAccess.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.WorkspaceId)
            .ToListAsync();
    }

    public async Task<WorkspaceAccess> GrantWorkspaceAccessAsync(Guid userId, Guid workspaceId, string accessLevel, Guid? invitedBy = null)
    {
        var existing = await db.UserWorkspaceAccess.FirstOrDefaultAsync(a => a.UserId == userId && a.WorkspaceId == workspaceId);

        if (existing != null)
        {
            existing.AccessLevel = AccessLevelHelper.Parse(accessLevel);
            existing.InvitedBy = invitedBy;
        }
        else
        {
            existing = new DbUserWorkspaceAccess
            {
                UserId = userId,
                WorkspaceId = workspaceId,
                AccessLevel = AccessLevelHelper.Parse(accessLevel),
                InvitedBy = invitedBy
            };
            db.UserWorkspaceAccess.Add(existing);
        }

        await db.SaveChangesAsync();

        return new WorkspaceAccess
        {
            UserId = existing.UserId,
            WorkspaceId = existing.WorkspaceId,
            AccessLevel = existing.AccessLevel.ToString(),
            InvitedBy = existing.InvitedBy
        };
    }

    public async Task<bool> UpdateWorkspaceAccessAsync(Guid userId, Guid workspaceId, string accessLevel)
    {
        var access = await db.UserWorkspaceAccess.FirstOrDefaultAsync(a => a.UserId == userId && a.WorkspaceId == workspaceId);
        if (access == null) return false;

        access.AccessLevel = AccessLevelHelper.Parse(accessLevel);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeWorkspaceAccessAsync(Guid userId, Guid workspaceId)
    {
        var access = await db.UserWorkspaceAccess.FirstOrDefaultAsync(a => a.UserId == userId && a.WorkspaceId == workspaceId);
        if (access == null) return false;

        db.UserWorkspaceAccess.Remove(access);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasWorkspaceAccessAsync(Guid userId, Guid workspaceId, string? minimumLevel = null)
    {
        var access = await db.UserWorkspaceAccess.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.WorkspaceId == workspaceId);

        if (access == null) return false;

        return AccessLevelHelper.MeetsMinimum(access.AccessLevel, minimumLevel);
    }

    // Organization Access
    public async Task<OrgAccess?> GetOrgAccessAsync(Guid userId, Guid orgId)
    {
        var access = await db.UserOrgAccess.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.OrgId == orgId);

        if (access == null) return null;

        return new OrgAccess
        {
            UserId = access.UserId,
            OrgId = access.OrgId,
            AccessLevel = access.AccessLevel.ToString(),
            InvitedBy = access.InvitedBy
        };
    }

    public async Task<List<OrgAccess>> GetOrgAccessListAsync(Guid orgId)
    {
        return await db.UserOrgAccess.AsNoTracking()
            .Where(a => a.OrgId == orgId)
            .Select(a => new OrgAccess
            {
                UserId = a.UserId,
                OrgId = a.OrgId,
                AccessLevel = a.AccessLevel.ToString(),
                InvitedBy = a.InvitedBy
            })
            .ToListAsync();
    }

    public async Task<List<Guid>> GetUserOrgIdsAsync(Guid userId)
    {
        return await db.UserOrgAccess.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.OrgId)
            .ToListAsync();
    }

    public async Task<OrgAccess> GrantOrgAccessAsync(Guid userId, Guid orgId, string accessLevel, Guid? invitedBy = null)
    {
        var existing = await db.UserOrgAccess.FirstOrDefaultAsync(a => a.UserId == userId && a.OrgId == orgId);

        if (existing != null)
        {
            existing.AccessLevel = AccessLevelHelper.Parse(accessLevel);
            existing.InvitedBy = invitedBy;
        }
        else
        {
            existing = new DbUserOrgAccess
            {
                UserId = userId,
                OrgId = orgId,
                AccessLevel = AccessLevelHelper.Parse(accessLevel),
                InvitedBy = invitedBy
            };
            db.UserOrgAccess.Add(existing);
        }

        await db.SaveChangesAsync();

        return new OrgAccess
        {
            UserId = existing.UserId,
            OrgId = existing.OrgId,
            AccessLevel = existing.AccessLevel.ToString(),
            InvitedBy = existing.InvitedBy
        };
    }

    public async Task<bool> UpdateOrgAccessAsync(Guid userId, Guid orgId, string accessLevel)
    {
        var access = await db.UserOrgAccess.FirstOrDefaultAsync(a => a.UserId == userId && a.OrgId == orgId);
        if (access == null) return false;

        access.AccessLevel = AccessLevelHelper.Parse(accessLevel);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeOrgAccessAsync(Guid userId, Guid orgId)
    {
        var access = await db.UserOrgAccess.FirstOrDefaultAsync(a => a.UserId == userId && a.OrgId == orgId);
        if (access == null) return false;

        db.UserOrgAccess.Remove(access);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasOrgAccessAsync(Guid userId, Guid orgId, string? minimumLevel = null)
    {
        var access = await db.UserOrgAccess.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.OrgId == orgId);

        if (access == null) return false;

        return AccessLevelHelper.MeetsMinimum(access.AccessLevel, minimumLevel);
    }

    // Workspace Invitations
    public async Task<WorkspaceInvitation?> GetWorkspaceInvitationAsync(Guid invitationId)
    {
        var inv = await db.WorkspaceInvitations.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (inv == null) return null;

        return new WorkspaceInvitation
        {
            Id = inv.Id,
            WorkspaceId = inv.WorkspaceId,
            TargetUserId = inv.TargetUserId,
            AccessLevel = inv.AccessLevel.ToString(),
            InviteMadeBy = inv.InviteMadeBy,
            Created = inv.Created,
            Expires = inv.Expires
        };
    }

    public async Task<List<WorkspaceInvitation>> GetUserWorkspaceInvitationsAsync(Guid userId)
    {
        return await db.WorkspaceInvitations.AsNoTracking()
            .Where(i => i.TargetUserId == userId && i.Expires > DateTime.UtcNow)
            .Select(i => new WorkspaceInvitation
            {
                Id = i.Id,
                WorkspaceId = i.WorkspaceId,
                TargetUserId = i.TargetUserId,
                AccessLevel = i.AccessLevel.ToString(),
                InviteMadeBy = i.InviteMadeBy,
                Created = i.Created,
                Expires = i.Expires
            })
            .ToListAsync();
    }

    public async Task<WorkspaceInvitation> CreateWorkspaceInvitationAsync(Guid workspaceId, Guid targetUserId, string accessLevel, Guid inviteMadeBy, DateTime? expires = null)
    {
        var inv = new DbWorkspaceInvitation
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            TargetUserId = targetUserId,
            AccessLevel = AccessLevelHelper.Parse(accessLevel),
            InviteMadeBy = inviteMadeBy,
            Created = DateTime.UtcNow,
            Expires = expires ?? DateTime.UtcNow.AddDays(7)
        };

        db.WorkspaceInvitations.Add(inv);
        await db.SaveChangesAsync();

        return new WorkspaceInvitation
        {
            Id = inv.Id,
            WorkspaceId = inv.WorkspaceId,
            TargetUserId = inv.TargetUserId,
            AccessLevel = inv.AccessLevel.ToString(),
            InviteMadeBy = inv.InviteMadeBy,
            Created = inv.Created,
            Expires = inv.Expires
        };
    }

    public async Task<bool> AcceptWorkspaceInvitationAsync(Guid invitationId, Guid userId)
    {
        var inv = await db.WorkspaceInvitations.FirstOrDefaultAsync(i => i.Id == invitationId && i.TargetUserId == userId);
        if (inv == null || inv.Expires < DateTime.UtcNow) return false;

        await GrantWorkspaceAccessAsync(userId, inv.WorkspaceId, inv.AccessLevel.ToString(), inv.InviteMadeBy);

        db.WorkspaceInvitations.Remove(inv);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeclineWorkspaceInvitationAsync(Guid invitationId, Guid userId)
    {
        var inv = await db.WorkspaceInvitations.FirstOrDefaultAsync(i => i.Id == invitationId && i.TargetUserId == userId);
        if (inv == null) return false;

        db.WorkspaceInvitations.Remove(inv);
        await db.SaveChangesAsync();
        return true;
    }

    // Org-Workspace ownership
    public async Task<bool> SetOrgWorkspaceOwnerAsync(Guid orgId, Guid workspaceId)
    {
        var existing = await db.OrgWorkspaceOwners.FirstOrDefaultAsync(o => o.OrgId == orgId && o.WorkspaceId == workspaceId);
        if (existing != null) return true;

        db.OrgWorkspaceOwners.Add(new DbOrgWorkspaceOwner { OrgId = orgId, WorkspaceId = workspaceId });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveOrgWorkspaceOwnerAsync(Guid orgId, Guid workspaceId)
    {
        var owner = await db.OrgWorkspaceOwners.FirstOrDefaultAsync(o => o.OrgId == orgId && o.WorkspaceId == workspaceId);
        if (owner == null) return false;

        db.OrgWorkspaceOwners.Remove(owner);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Guid?> GetWorkspaceOrgIdAsync(Guid workspaceId)
    {
        var owner = await db.OrgWorkspaceOwners.AsNoTracking()
            .FirstOrDefaultAsync(o => o.WorkspaceId == workspaceId);

        return owner?.OrgId;
    }

    public async Task<List<Guid>> GetOrgWorkspaceIdsAsync(Guid orgId)
    {
        return await db.OrgWorkspaceOwners.AsNoTracking()
            .Where(o => o.OrgId == orgId)
            .Select(o => o.WorkspaceId)
            .ToListAsync();
    }
}

public class EfOrganizationRepository(GrahpletDbContext db) : IOrganizationRepository
{
    public async Task<List<Organization>> GetUserOrganizationsAsync(Guid userId)
    {
        var orgIds = await db.UserOrgAccess.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.OrgId)
            .ToListAsync();

        return await db.Organizations.AsNoTracking()
            .Where(o => orgIds.Contains(o.Id))
            .Select(o => new Organization { Id = o.Id, Name = o.Name })
            .ToListAsync();
    }

    public async Task<Organization?> GetOrganizationAsync(Guid userId, Guid orgId)
    {
        // No auth check - controller handles this
        var org = await db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orgId);
        if (org == null) return null;

        return new Organization { Id = org.Id, Name = org.Name };
    }

    public async Task<Organization> CreateOrganizationAsync(Guid userId, OrganizationCreate org)
    {
        var e = new DbOrganization { Id = Guid.NewGuid(), Name = org.Name };
        db.Organizations.Add(e);

        // Grant owner access to creator
        db.UserOrgAccess.Add(new DbUserOrgAccess
        {
            UserId = userId,
            OrgId = e.Id,
            AccessLevel = AccessLevel.Owner,
            InvitedBy = null
        });

        await db.SaveChangesAsync();
        return new Organization { Id = e.Id, Name = e.Name };
    }

    public async Task<Organization?> UpdateOrganizationAsync(Guid userId, Guid orgId, OrganizationUpdate org)
    {
        // No auth check - controller handles this
        var e = await db.Organizations.FirstOrDefaultAsync(o => o.Id == orgId);
        if (e == null) return null;

        if (!string.IsNullOrWhiteSpace(org.Name)) e.Name = org.Name!;
        await db.SaveChangesAsync();

        return new Organization { Id = e.Id, Name = e.Name };
    }

    public async Task<bool> DeleteOrganizationAsync(Guid userId, Guid orgId)
    {
        // No auth check - controller handles this
        var e = await db.Organizations.FirstOrDefaultAsync(o => o.Id == orgId);
        if (e == null) return false;

        // Remove all access entries
        var accessEntries = db.UserOrgAccess.Where(a => a.OrgId == orgId);
        db.UserOrgAccess.RemoveRange(accessEntries);

        // Remove org workspace ownership
        var workspaceOwnership = db.OrgWorkspaceOwners.Where(o => o.OrgId == orgId);
        db.OrgWorkspaceOwners.RemoveRange(workspaceOwnership);

        db.Organizations.Remove(e);
        await db.SaveChangesAsync();
        return true;
    }
}
