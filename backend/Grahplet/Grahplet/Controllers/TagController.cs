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

    public TagController(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
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
        var tag = await _tagRepository.CreateTagAsync(userId, workspaceId, request);
        return CreatedAtAction(nameof(GetTag), new { workspaceId, tagId = tag.Id }, tag);
    }

    [HttpGet("{tagId}")]
    public async Task<IActionResult> GetTag(Guid workspaceId, Guid tagId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var tag = await _tagRepository.GetTagAsync(userId, workspaceId, tagId);
        
        if (tag == null)
        {
            return NotFound("Tag not found or access denied");
        }

        return Ok(tag);
    }

    [HttpPut("{tagId}")]
    public async Task<IActionResult> UpdateTag(Guid workspaceId, Guid tagId, [FromBody] TagUpdate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var tag = await _tagRepository.UpdateTagAsync(userId, workspaceId, tagId, request);
        
        if (tag == null)
        {
            return NotFound("Tag not found or access denied");
        }

        return Ok(tag);
    }

    [HttpDelete("{tagId}")]
    public async Task<IActionResult> DeleteTag(Guid workspaceId, Guid tagId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var success = await _tagRepository.DeleteTagAsync(userId, workspaceId, tagId);
        
        if (!success)
        {
            return NotFound("Tag not found or access denied");
        }

        return NoContent();
    }
}

