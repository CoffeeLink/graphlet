namespace Grahplet.Models;

public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WorkspaceCreate
{
    public string Name { get; set; } = string.Empty;
}

public class WorkspaceUpdate
{
    public string? Name { get; set; }
}

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int[] Color { get; set; } = new int[3]; // RGB
}

public class TagCreate
{
    public string Name { get; set; } = string.Empty;
    public int[] Color { get; set; } = new int[3];
}

public class TagUpdate
{
    public string? Name { get; set; }
    public int[]? Color { get; set; }
}

public class FileInfo
{
    public Guid Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; // img, video, audio, other
    public string ContentUrl { get; set; } = string.Empty;
}

public class Note
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // txt, url, file
    public string? Value { get; set; }
    public FileInfo? File { get; set; }
    public List<Tag> Tags { get; set; } = new();
    public List<NoteRelation> Relations { get; set; } = new();
}

public class NoteCreate
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // txt, url, file
    public string? Value { get; set; }
    public FileInfo? File { get; set; }
    public List<Guid>? Tags { get; set; }
}

public class NoteUpdate
{
    public string? Name { get; set; }
    public string? Kind { get; set; }
    public string? Value { get; set; }
    public FileInfo? File { get; set; }
}

public class NoteRelation
{
    public Guid Id { get; set; }
    public Guid[] Connection { get; set; } = new Guid[2]; // Two note IDs
    public string Name { get; set; } = string.Empty;
}

public class NoteRelationCreate
{
    public Guid OtherId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class NoteRelationUpdate
{
    public string? Name { get; set; }
}

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ProfilePicUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
}

public class PublicUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ProfilePicUrl { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

