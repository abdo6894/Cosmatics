using System.Security.Claims;
using MoveLens.Application.Common.Interfaces;

namespace MoveLens.Api.Services;


public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id =>
        httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContextAccessor.HttpContext?.User
            .FindFirstValue("sub");
}