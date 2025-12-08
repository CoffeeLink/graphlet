using Grahplet.Models;
using Grahplet.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Grahplet.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;

    public AuthController(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required");
        }

        var (success, token) = await _authRepository.LoginAsync(request.Email, request.Password);
        
        if (!success)
        {
            return Unauthorized("Invalid credentials");
        }

        Response.Headers["Authorization"] = token;
        return Ok(new { token });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader))
        {
            return Unauthorized("Missing Authorization header");
        }

        var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
        var isValid = await _authRepository.ValidateTokenAsync(token);
        
        if (!isValid)
        {
            return Unauthorized("Invalid token");
        }

        await _authRepository.LogoutAsync(token);
        return Ok("Successfully logged out");
    }
}

