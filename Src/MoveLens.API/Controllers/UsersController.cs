using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoveLens.Api.Extensions;
using MoveLens.Api.Models;
using MoveLens.Application.Common.Interfaces;
using MoveLens.Application.Features.Users.Commands.Deactivate;
using MoveLens.Application.Features.Users.Commands.UpdatePreferences;
using MoveLens.Application.Features.Users.Commands.UpdateProfile;
using MoveLens.Application.Features.Users.Dtos;
using MoveLens.Application.Features.Users.Queries.GetProfile;

namespace MoveLens.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class UsersController(ISender sender, IUser currentUser) : ControllerBase
{
    [HttpGet("GetUserProfile")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        
        if (string.IsNullOrEmpty(currentUser.Id))
            return Unauthorized(ApiResponse<object>.FailResponse("User identity could not be resolved."));

      
        var result = await sender.Send(new GetUserProfileQuery(currentUser.Id), ct);
        return result.ToActionResult(profile =>
            Ok(ApiResponse<UserProfileResponse>.SuccessResponse(profile)));
    }

 

    [HttpPut("UpdateUserProfile")]

    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<object>.FailResponse("User identity could not be resolved."));

        var result = await sender.Send(new UpdateUserProfileCommand(userId, request.FullName), ct);
        return result.ToActionResult(_ => NoContent());
    }

    [HttpPut("UpdateUserPrefrences")]

    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateUserPreferencesRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<object>.FailResponse("User identity could not be resolved."));

        var result = await sender.Send(new UpdateUserPreferencesCommand(
            userId,
            request.PreferredMoods,
            request.Language,
            request.MaxBudget,
            request.FavoriteGovernorates), ct);
        return result.ToActionResult(_ => NoContent());
    }

    [HttpDelete("Deactivate")]
    public async Task<IActionResult> Deactivate(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<object>.FailResponse("User identity could not be resolved."));

        var result = await sender.Send(new DeactivateUserCommand(userId), ct);
        return result.ToActionResult(_ => NoContent());
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = default;
        return currentUser.Id is not null
            && Guid.TryParse(currentUser.Id, out userId);
    }
}