using Cosmatics.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Cosmatics.API.MiddleWare;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> PublicPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/verify-otp",
        "/api/auth/resend-otp",
        "/api/auth/forgot-password",
        "/api/auth/reset-password",
        "/api/auth/refresh-token",
        "/api/countries",
        "/api/sliders",
        "/api/categories",
        "/api/products"
    };

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && PublicPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

       
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }


        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var authService = context.RequestServices.GetRequiredService<IAuthService>();
                var user = await authService.GetUserByIdAsync(userId);

                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (user == null || user.ActiveToken != token)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked or is invalid." });
                    return;
                }
            }
        }

        await _next(context);
    }
}