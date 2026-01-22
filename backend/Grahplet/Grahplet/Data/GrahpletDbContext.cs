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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbUser>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<DbSessionToken>()
            .HasKey(t => t.Token);

        modelBuilder.Entity<DbWorkspace>()
            .HasIndex(w => new { w.UserId, w.Name });

        modelBuilder.Entity<DbTag>()
            .HasIndex(t => new { t.UserId, t.WorkspaceId, t.Name });

        modelBuilder.Entity<DbNoteTag>()
            .HasKey(nt => new { nt.NoteId, nt.TagId });

        modelBuilder.Entity<DbNoteRelation>()
            .HasIndex(r => new { r.UserId, r.Note1Id, r.Note2Id });
    }
}
