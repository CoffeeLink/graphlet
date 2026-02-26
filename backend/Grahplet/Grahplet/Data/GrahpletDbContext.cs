using Microsoft.EntityFrameworkCore;

namespace Grahplet.Data;

public class GrahpletDbContext(DbContextOptions<GrahpletDbContext> options) : DbContext(options)
{
    public DbSet<DbUser> Users => Set<DbUser>();
    public DbSet<DbSessionToken> SessionTokens => Set<DbSessionToken>();
    public DbSet<DbWorkspace> Workspaces => Set<DbWorkspace>();
    public DbSet<DbTag> Tags => Set<DbTag>();
    public DbSet<DbNote> Notes => Set<DbNote>();
    public DbSet<DbNoteTag> NoteTags => Set<DbNoteTag>();
    public DbSet<DbNoteRelation> NoteRelations => Set<DbNoteRelation>();

    // Access control
    public DbSet<DbUserWorkspaceAccess> UserWorkspaceAccess => Set<DbUserWorkspaceAccess>();
    public DbSet<DbWorkspaceInvitation> WorkspaceInvitations => Set<DbWorkspaceInvitation>();

    // Organizations
    public DbSet<DbOrganization> Organizations => Set<DbOrganization>();
    public DbSet<DbUserOrgAccess> UserOrgAccess => Set<DbUserOrgAccess>();
    public DbSet<DbOrgWorkspaceOwner> OrgWorkspaceOwners => Set<DbOrgWorkspaceOwner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbUser>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<DbSessionToken>()
            .HasKey(t => t.Token);

        modelBuilder.Entity<DbWorkspace>()
            .HasIndex(w => w.Name);

        modelBuilder.Entity<DbTag>()
            .HasIndex(t => new { t.WorkspaceId, t.Name });

        modelBuilder.Entity<DbNoteTag>()
            .HasKey(nt => new { nt.NoteId, nt.TagId });

        modelBuilder.Entity<DbNoteRelation>()
            .HasIndex(r => new { r.Note1Id, r.Note2Id });

        // Access control composite keys
        modelBuilder.Entity<DbUserWorkspaceAccess>()
            .HasKey(a => new { a.UserId, a.WorkspaceId });

        modelBuilder.Entity<DbUserOrgAccess>()
            .HasKey(a => new { a.UserId, a.OrgId });

        modelBuilder.Entity<DbOrgWorkspaceOwner>()
            .HasKey(o => new { o.OrgId, o.WorkspaceId });

        // Indexes for access queries
        modelBuilder.Entity<DbUserWorkspaceAccess>()
            .HasIndex(a => a.UserId);

        modelBuilder.Entity<DbUserWorkspaceAccess>()
            .HasIndex(a => a.WorkspaceId);

        modelBuilder.Entity<DbUserOrgAccess>()
            .HasIndex(a => a.UserId);

        modelBuilder.Entity<DbUserOrgAccess>()
            .HasIndex(a => a.OrgId);
    }
}
