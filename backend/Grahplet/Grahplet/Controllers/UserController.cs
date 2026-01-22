using Grahplet.Extensions;
using Grahplet.Models;
using Grahplet.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Grahplet.Controllers;


public class UserCreate
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserUpdate
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? ProfilePicUrl { get; set; }
}

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IAuthRepository _authRepository;

    public UserController(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    private IActionResult RequireAuth()
    {
        if (HttpContext.GetUserId() == null)
        {
            return Unauthorized("Missing or invalid Authorization header");
        }
        return null!;
    }

    // Get a public profile of another user by id (no auth required)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPublicUser(Guid id)
    {
        var user = await _authRepository.GetUserAsync(id);
        if (user == null) return NotFound("User not found");

        var publicUser = new PublicUser
        {
            Id = user.Id,
            Username = user.Username,
            ProfilePicUrl = user.ProfilePicUrl
        };

        return Ok(publicUser);
    }

    // Register a new user
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserCreate req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Name, Email and Password are required");
        }

        var created = await _authRepository.CreateUserAsync(req.Name, req.Email, req.Password);
        if (created == null)
        {
            return Conflict("Username or email already exists");
        }

        // Do not expose password in response
        created.Password = string.Empty;
        return StatusCode(StatusCodes.Status201Created, created);
    }

    // Get current user profile
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();
        var user = await _authRepository.GetUserAsync(userId);
        if (user == null) return NotFound("User not found");
        user.Password = string.Empty;
        return Ok(user);
    }

    // Update current user profile (partial)
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UserUpdate update)
    {
        var authCheck = RequireAuth();
        if (authCheck != null) return authCheck;

        var userId = HttpContext.GetRequiredUserId();

        // Map the partial update into Models.User so repository can apply changes
        var patch = new User
        {
            Username = update.Username ?? string.Empty,
            Email = update.Email ?? string.Empty,
            ProfilePicUrl = update.ProfilePicUrl ?? string.Empty
        };

        var updated = await _authRepository.UpdateUserAsync(userId, patch);
        if (updated == null) return NotFound("User not found");
        updated.Password = string.Empty;
        return Ok(updated);
    }
}