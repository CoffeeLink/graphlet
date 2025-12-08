using Grahplet.Repositories;

namespace Grahplet.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthRepository authRepository)
    {
        // Skip auth for login endpoint
        if (context.Request.Path.StartsWithSegments("/api/login"))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(authHeader))
        {
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
            var isValid = await authRepository.ValidateTokenAsync(token);
            
            if (isValid)
            {
                var userId = await authRepository.GetUserIdFromTokenAsync(token);
                if (userId.HasValue)
                {
                    context.Items["UserId"] = userId.Value;
                }
            }
        }

        await _next(context);
    }
}

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}

