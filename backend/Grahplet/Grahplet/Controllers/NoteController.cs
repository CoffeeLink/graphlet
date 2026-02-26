using Grahplet.Models;
using Grahplet.Repositories;
using Grahplet.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Grahplet.Controllers;

[ApiController]
[Route("api/workspace/{workspaceId}/note")]
public class NoteController : ControllerBase
{
    private readonly INoteRepository _noteRepository;
    private readonly IAccessRepository _accessRepository;

    public NoteController(INoteRepository noteRepository, IAccessRepository accessRepository)
    {
        _noteRepository = noteRepository;
        _accessRepository = accessRepository;
    }

    private IActionResult RequireAuth()
    {
        if (HttpContext.GetUserId() == null)
        {
            return Unauthorized("Missing or invalid Authorization header");
        }
        return null!;
    }

    [HttpGet]
    public async Task<IActionResult> ListNotes(Guid workspaceId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId);
        if (!hasAccess)
        {
            return StatusCode(403, "Access denied to workspace");
        }

        var notes = await _noteRepository.GetNotesAsync(userId, workspaceId);
        return Ok(notes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote(Guid workspaceId, [FromBody] NoteCreate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Kind))
        {
            return BadRequest("Name and kind are required");
        }

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var note = await _noteRepository.CreateNoteAsync(userId, workspaceId, request);
        return CreatedAtAction(nameof(GetNote), new { workspaceId, noteId = note.Id }, note);
    }

    [HttpGet("{noteId}")]
    public async Task<IActionResult> GetNote(Guid workspaceId, Guid noteId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId);
        if (!hasAccess)
        {
            return StatusCode(403, "Access denied to workspace");
        }

        var note = await _noteRepository.GetNoteAsync(userId, workspaceId, noteId);
        
        if (note == null)
        {
            return NotFound("Note not found");
        }

        return Ok(note);
    }

    [HttpPut("{noteId}")]
    public async Task<IActionResult> UpdateNote(Guid workspaceId, Guid noteId, [FromBody] NoteUpdate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var note = await _noteRepository.UpdateNoteAsync(userId, workspaceId, noteId, request);
        
        if (note == null)
        {
            return NotFound("Note not found");
        }

        return Ok(note);
    }

    [HttpDelete("{noteId}")]
    public async Task<IActionResult> DeleteNote(Guid workspaceId, Guid noteId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var success = await _noteRepository.DeleteNoteAsync(userId, workspaceId, noteId);
        
        if (!success)
        {
            return NotFound("Note not found");
        }

        return NoContent();
    }

    [HttpGet("{noteId}/tags/{tagId}")]
    public async Task<IActionResult> GetNoteTag(Guid workspaceId, Guid noteId, Guid tagId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId);
        if (!hasAccess)
        {
            return StatusCode(403, "Access denied to workspace");
        }

        var note = await _noteRepository.GetNoteAsync(userId, workspaceId, noteId);
        
        if (note == null)
        {
            return NotFound("Note not found");
        }

        var tag = note.Tags.FirstOrDefault(t => t.Id == tagId);
        
        if (tag == null)
        {
            return NotFound("Tag not found in note");
        }

        return Ok(tag);
    }

    [HttpPut("{noteId}/tags/{tagId}")]
    public async Task<IActionResult> AttachTagToNote(Guid workspaceId, Guid noteId, Guid tagId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var note = await _noteRepository.AttachTagToNoteAsync(userId, workspaceId, noteId, tagId);
        
        if (note == null)
        {
            return NotFound("Note not found");
        }

        return Ok(note);
    }

    [HttpDelete("{noteId}/tags/{tagId}")]
    public async Task<IActionResult> DetachTagFromNote(Guid workspaceId, Guid noteId, Guid tagId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var success = await _noteRepository.DetachTagFromNoteAsync(userId, workspaceId, noteId, tagId);
        
        if (!success)
        {
            return NotFound("Note or tag not found");
        }

        return NoContent();
    }

    [HttpPost("{noteId}/relation")]
    public async Task<IActionResult> CreateRelation(Guid workspaceId, Guid noteId, [FromBody] NoteRelationCreate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        if (request.OtherId == Guid.Empty || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("OtherId and name are required");
        }

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var relation = await _noteRepository.CreateRelationAsync(userId, workspaceId, noteId, request);
        return CreatedAtAction(nameof(GetRelation), new { workspaceId, noteId, relationId = relation.Id }, relation);
    }

    [HttpGet("{noteId}/relation/{relationId}")]
    public async Task<IActionResult> GetRelation(Guid workspaceId, Guid noteId, Guid relationId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId);
        if (!hasAccess)
        {
            return StatusCode(403, "Access denied to workspace");
        }

        var relation = await _noteRepository.GetRelationAsync(userId, workspaceId, noteId, relationId);
        
        if (relation == null)
        {
            return NotFound("Relation not found");
        }

        return Ok(relation);
    }

    [HttpPut("{noteId}/relation/{relationId}")]
    public async Task<IActionResult> UpdateRelation(Guid workspaceId, Guid noteId, Guid relationId, [FromBody] NoteRelationUpdate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var relation = await _noteRepository.UpdateRelationAsync(userId, workspaceId, noteId, relationId, request);
        
        if (relation == null)
        {
            return NotFound("Relation not found");
        }

        return Ok(relation);
    }

    [HttpDelete("{noteId}/relation/{relationId}")]
    public async Task<IActionResult> DeleteRelation(Guid workspaceId, Guid noteId, Guid relationId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var success = await _noteRepository.DeleteRelationAsync(userId, workspaceId, noteId, relationId);
        
        if (!success)
        {
            return NotFound("Relation not found");
        }

        return NoContent();
    }
}