using Grahplet.Models;
using Grahplet.Repositories;
using Grahplet.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Grahplet.Controllers;

[ApiController]
[Route("api/workspace")]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceController(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
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
    public async Task<IActionResult> ListWorkspaces()
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var workspaces = await _workspaceRepository.GetWorkspacesAsync(userId);
        return Ok(workspaces);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkspace([FromBody] WorkspaceCreate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required");
        }

        var userId = HttpContext.GetRequiredUserId();
        var workspace = await _workspaceRepository.CreateWorkspaceAsync(userId, request);
        return CreatedAtAction(nameof(GetWorkspace), new { id = workspace.Id }, workspace);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkspace(Guid id)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var workspace = await _workspaceRepository.GetWorkspaceAsync(userId, id);
        
        if (workspace == null)
        {
            return NotFound("Workspace not found or access denied");
        }

        return Ok(workspace);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] WorkspaceUpdate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var workspace = await _workspaceRepository.UpdateWorkspaceAsync(userId, id, request);
        
        if (workspace == null)
        {
            return NotFound("Workspace not found or access denied");
        }

        return Ok(workspace);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkspace(Guid id)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var success = await _workspaceRepository.DeleteWorkspaceAsync(userId, id);
        
        if (!success)
        {
            return NotFound("Workspace not found or access denied");
        }

        return NoContent();
    }
}

