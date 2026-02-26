using Grahplet.Models;
using Grahplet.Repositories;
using Grahplet.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Grahplet.Controllers;

[ApiController]
[Route("api/organization")]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IAccessRepository _accessRepository;

    public OrganizationController(
        IOrganizationRepository organizationRepository,
        IAccessRepository accessRepository)
    {
        _organizationRepository = organizationRepository;
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
    public async Task<IActionResult> ListOrganizations()
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var organizations = await _organizationRepository.GetUserOrganizationsAsync(userId);
        return Ok(organizations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] OrganizationCreate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required");
        }

        var userId = HttpContext.GetRequiredUserId();
        var organization = await _organizationRepository.CreateOrganizationAsync(userId, request);
        return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrganization(Guid id)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check access
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, id);
        if (!hasAccess)
        {
            return NotFound("Organization not found or access denied");
        }

        var organization = await _organizationRepository.GetOrganizationAsync(userId, id);

        if (organization == null)
        {
            return NotFound("Organization not found");
        }

        return Ok(organization);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] OrganizationUpdate request)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Admin access
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, id, "Admin");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Admin access required");
        }

        var organization = await _organizationRepository.UpdateOrganizationAsync(userId, id, request);

        if (organization == null)
        {
            return NotFound("Organization not found");
        }

        return Ok(organization);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check Owner access
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, id, "Owner");
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions - Owner access required");
        }

        var success = await _organizationRepository.DeleteOrganizationAsync(userId, id);

        if (!success)
        {
            return NotFound("Organization not found");
        }

        return NoContent();
    }

    [HttpGet("{id}/workspaces")]
    public async Task<IActionResult> GetOrganizationWorkspaces(Guid id)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Check if user has access to the organization
        var hasAccess = await _accessRepository.HasOrgAccessAsync(userId, id);
        if (!hasAccess)
        {
            return StatusCode(403, "Insufficient permissions");
        }

        var workspaceIds = await _accessRepository.GetOrgWorkspaceIdsAsync(id);
        return Ok(workspaceIds);
    }
}
