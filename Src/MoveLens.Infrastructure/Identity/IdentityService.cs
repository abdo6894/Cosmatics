using Microsoft.AspNetCore.Identity;
using MoveLens.Application.Common.Interfaces;
using MoveLens.Application.Features.Identity.Dtos;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager)
    : IIdentityService
{
    public async Task<Result<AppUserDto>> CreateAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Error.Conflict(
                "Auth.EmailAlreadyExists",
                "An account with this email already exists.");

        var user = new AppUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var err = result.Errors.First();
            return Error.Validation(err.Code, err.Description);
        }

        await userManager.AddToRoleAsync(user, "User");

        return await BuildAppUserDtoAsync(user);
    }

    public async Task<Result<AppUserDto>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Email or password is incorrect.");

        var result = await signInManager.CheckPasswordSignInAsync(
            user, password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Error.Forbidden(
                "Auth.AccountLockedOut",
                "Account is locked. Please try again later.");

        if (!result.Succeeded)
            return Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Email or password is incorrect.");

        return await BuildAppUserDtoAsync(user);
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(
        string userId,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound(
                "Auth.UserNotFound",
                "User not found.");

        return await BuildAppUserDtoAsync(user);
    }

    public async Task<bool> IsInRoleAsync(
        string userId,
        string role,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is not null && await userManager.IsInRoleAsync(user, role);
    }

    public async Task<string?> GetUserNameAsync(
        string userId,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    // ── Private ───────────────────────────────────────────────────────────

    private async Task<AppUserDto> BuildAppUserDtoAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        return new AppUserDto(user.Id, user.Email!, roles, claims);
    }
}