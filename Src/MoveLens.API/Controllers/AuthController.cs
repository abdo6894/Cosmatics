using MediatR;
using Microsoft.AspNetCore.Mvc;
using MoveLens.Api.Extensions;
using MoveLens.Api.Models;
using MoveLens.Application.Features.Identity.Queries.RefreshToken;
using MoveLens.Application.Features.Identity.Queries.RefreshTokens;
using MoveLens.Application.Features.Users.Commands.Register;
using MoveLens.Application.Features.Users.Dtos;
using MoveLens.Application.Features.Users.Queries.Login;

namespace MoveLens.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController(ISender sender) : ControllerBase
{


    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RegisterUserCommand(request.FullName, request.Email, request.Password), ct);

        return result.ToActionResult(value =>
            CreatedAtAction(
                actionName: nameof(UsersController.GetMyProfile),
                controllerName: "Users",
                routeValues: null,
                value: ApiResponse<RegisterUserResponse>.SuccessResponse(
                                    value, "User registered successfully.")));
    }


 
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new LoginQuery(request.Email, request.Password), ct);

        return result.ToActionResult(token =>
            Ok(ApiResponse<LoginResponse>.SuccessResponse(
                new LoginResponse(token.AccessToken!, token.RefreshToken!, token.ExpiresOnUtc),
                "Login successful.")));
    }

    


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RefreshTokenQuery(request.RefreshToken, request.ExpiredAccessToken), ct);

        return result.ToActionResult(token =>
            Ok(ApiResponse<LoginResponse>.SuccessResponse(
                new LoginResponse(token.AccessToken!, token.RefreshToken!, token.ExpiresOnUtc),
                "Token refreshed successfully.")));
    }
}



public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken, string ExpiredAccessToken);
public sealed record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresOnUtc);