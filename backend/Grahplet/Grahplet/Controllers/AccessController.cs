using Grahplet.Models;
using Grahplet.Repositories;
using Grahplet.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Grahplet.Controllers;

[ApiController]
[Route("api/access")]
public class AccessController : ControllerBase
{
    private readonly IAccessRepository _accessRepository;

    public AccessController(IAccessRepository accessRepository)
    {
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

    // Workspace Access Endpoints
    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetWorkspaceAccess(Guid workspaceId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var access = await _accessRepository.GetWorkspaceAccessAsync(userId, workspaceId);

        if (access == null)
        {
            return NotFound("Access not found");
        }

        return Ok(access);
    }

    [HttpGet("workspace/{workspaceId}/list")]
    public async Task<IActionResult> ListWorkspaceAccess(Guid workspaceId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to view all access entries
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var accessList = await _accessRepository.GetWorkspaceAccessListAsync(workspaceId);
        return Ok(accessList);
    }

    [HttpPost("workspace/{workspaceId}/grant")]
    public async Task<IActionResult> GrantWorkspaceAccess(Guid workspaceId, [FromBody] GrantAccessRequest request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to grant access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var access = await _accessRepository.GrantWorkspaceAccessAsync(
            request.TargetUserId,
            workspaceId,
            request.AccessLevel,
            userId
        );

        return Ok(access);
    }

    [HttpPut("workspace/{workspaceId}/update")]
    public async Task<IActionResult> UpdateWorkspaceAccess(Guid workspaceId, [FromBody] UpdateAccessRequest request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to update access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var success = await _accessRepository.UpdateWorkspaceAccessAsync(
            request.TargetUserId,
            workspaceId,
            request.AccessLevel
        );

        if (!success)
        {
            return NotFound("Access not found");
        }

        return Ok();
    }

    [HttpDelete("workspace/{workspaceId}/revoke/{targetUserId}")]
    public async Task<IActionResult> RevokeWorkspaceAccess(Guid workspaceId, Guid targetUserId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to revoke access
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var success = await _accessRepository.RevokeWorkspaceAccessAsync(targetUserId, workspaceId);

        if (!success)
        {
            return NotFound("Access not found");
        }

        return NoContent();
    }

    // Organization Access Endpoints
    [HttpGet("organization/{orgId}")]
    public async Task<IActionResult> GetOrgAccess(Guid orgId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var access = await _accessRepository.GetOrgAccessAsync(userId, orgId);

        if (access == null)
        {
            return NotFound("Access not found");
        }

        return Ok(access);
    }

    [HttpGet("organization/{orgId}/list")]
    public async Task<IActionResult> ListOrgAccess(Guid orgId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to view all access entries
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, orgId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var accessList = await _accessRepository.GetOrgAccessListAsync(orgId);
        return Ok(accessList);
    }

    [HttpPost("organization/{orgId}/grant")]
    public async Task<IActionResult> GrantOrgAccess(Guid orgId, [FromBody] GrantAccessRequest request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to grant access
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, orgId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var access = await _accessRepository.GrantOrgAccessAsync(
            request.TargetUserId,
            orgId,
            request.AccessLevel,
            userId
        );

        return Ok(access);
    }

    [HttpDelete("organization/{orgId}/revoke/{targetUserId}")]
    public async Task<IActionResult> RevokeOrgAccess(Guid orgId, Guid targetUserId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to revoke access
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, orgId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var success = await _accessRepository.RevokeOrgAccessAsync(targetUserId, orgId);

        if (!success)
        {
            return NotFound("Access not found");
        }

        return NoContent();
    }

    // Workspace Invitations
    [HttpGet("invitations")]
    public async Task<IActionResult> GetMyInvitations()
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var invitations = await _accessRepository.GetUserWorkspaceInvitationsAsync(userId);

        return Ok(invitations);
    }

    [HttpPost("workspace/{workspaceId}/invite")]
    public async Task<IActionResult> CreateWorkspaceInvitation(Guid workspaceId, [FromBody] WorkspaceInvitationCreate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has admin access to invite
        var hasAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var invitation = await _accessRepository.CreateWorkspaceInvitationAsync(
            workspaceId,
            request.TargetUserId,
            request.AccessLevel,
            userId
        );

        return Ok(invitation);
    }

    [HttpPost("invitations/{invitationId}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid invitationId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var success = await _accessRepository.AcceptWorkspaceInvitationAsync(invitationId, userId);

        if (!success)
        {
            return NotFound("Invitation not found or expired");
        }

        return Ok();
    }

    [HttpPost("invitations/{invitationId}/decline")]
    public async Task<IActionResult> DeclineInvitation(Guid invitationId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var success = await _accessRepository.DeclineWorkspaceInvitationAsync(invitationId, userId);

        if (!success)
        {
            return NotFound("Invitation not found");
        }

        return Ok();
    }

    // Org-Workspace ownership
    [HttpPost("organization/{orgId}/workspace/{workspaceId}")]
    public async Task<IActionResult> SetOrgWorkspaceOwner(Guid orgId, Guid workspaceId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has owner access to org
        var hasOrgAccess = await _accessRepository.HasOrgAccessAsync(userId, orgId, "Owner");
        if (!hasOrgAccess)
        {
            return StatusCode(403, "Insufficient permissions on organization");
        }

        // Check if user has owner access to workspace
        var hasWorkspaceAccess = await _accessRepository.HasWorkspaceAccessAsync(userId, workspaceId, "Owner");
        if (!hasWorkspaceAccess)
        {
            return StatusCode(403, "Insufficient permissions on workspace");
        }

        var success = await _accessRepository.SetOrgWorkspaceOwnerAsync(orgId, workspaceId);

        return Ok(new { success });
    }

    [HttpDelete("organization/{orgId}/workspace/{workspaceId}")]
    public async Task<IActionResult> RemoveOrgWorkspaceOwner(Guid orgId, Guid workspaceId)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has owner access to org
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, orgId, "Owner");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var success = await _accessRepository.RemoveOrgWorkspaceOwnerAsync(orgId, workspaceId);

        if (!success)
        {
            return NotFound("Ownership not found");
        }

        return NoContent();
    }
}

// Request models
public class GrantAccessRequest
{
    public Guid TargetUserId { get; set; }
    public string AccessLevel { get; set; } = string.Empty;
}

public class UpdateAccessRequest
{
    public Guid TargetUserId { get; set; }
    public string AccessLevel { get; set; } = string.Empty;
}