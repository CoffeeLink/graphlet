using Grahplet.Models;
using Grahplet.Repositories;
using Grahplet.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Grahplet.Controllers;

[ApiController]
[Route("api/workspace/{workspaceId}/tags")]
public class TagController : ControllerBase
{
    private readonly ITagRepository _tagRepository;
    private readonly IAccessRepository _accessRepository;

    public TagController(ITagRepository tagRepository, IAccessRepository accessRepository)
    {
        _tagRepository = tagRepository;
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
    public async Task<IActionResult> ListTags(Guid workspaceId)
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

        var tags = await _tagRepository.GetTagsAsync(userId, workspaceId);
        return Ok(tags);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag(Guid workspaceId, [FromBody] TagCreate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        if (string.IsNullOrWhiteSpace(request.Name) || request.Color == null || request.Color.Length != 3)
        {
            return BadRequest("Name and color (RGB array) are required");
        }

        var userId = HttpContext.GetRequiredUserId();

        // Check Write access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Write");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Write access required");
        }

        var tag = await _tagRepository.CreateTagAsync(userId, workspaceId, request);
        return CreatedAtAction(nameof(GetTag), new { workspaceId, tagId = tag.Id }, tag);
    }

    [HttpGet("{tagId}")]
    public async Task<IActionResult> GetTag(Guid workspaceId, Guid tagId)
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

        var tag = await _tagRepository.GetTagAsync(userId, workspaceId, tagId);
        
        if (tag == null)
        {
            return NotFound("Tag not found");
        }

        return Ok(tag);
    }

    [HttpPut("{tagId}")]
    public async Task<IActionResult> UpdateTag(Guid workspaceId, Guid tagId, [FromBody] TagUpdate request)
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

        var tag = await _tagRepository.UpdateTagAsync(userId, workspaceId, tagId, request);
        
        if (tag == null)
        {
            return NotFound("Tag not found");
        }

        return Ok(tag);
    }

    [HttpDelete("{tagId}")]
    public async Task<IActionResult> DeleteTag(Guid workspaceId, Guid tagId)
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

        var success = await _tagRepository.DeleteTagAsync(userId, workspaceId, tagId);
        
        if (!success)
        {
            return NotFound("Tag not found");
        }

        return NoContent();
    }
}

