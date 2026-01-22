using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grahplet.Data;

public class DbUser
{
    [Key]
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;
    [MaxLength(500)]
    public string ProfilePicUrl { get; set; } = string.Empty;
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;
    // PBKDF2 hash stored as: v1$iterations$saltBase64$hashBase64
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
}

public class DbSessionToken
{
    [Key]
    [MaxLength(200)]
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class DbWorkspace
{
    [Key]
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class DbTag
{
    [Key]
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    // stored as "r,g,b"
    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
}

public class DbNote
{
    [Key]
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Kind { get; set; } = string.Empty;
    public string? Value { get; set; }
    // FileInfo flattened
    public Guid? File_Id { get; set; }
    [MaxLength(300)]
    public string? File_Filename { get; set; }
    [MaxLength(100)]
    public string? File_ContentType { get; set; }
    [MaxLength(1000)]
    public string? File_ContentUrl { get; set; }
    public Guid UserId { get; set; }
}

public class DbNoteTag
{
    public Guid NoteId { get; set; }
    public Guid TagId { get; set; }
}

public class DbNoteRelation
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid Note1Id { get; set; }
    public Guid Note2Id { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
