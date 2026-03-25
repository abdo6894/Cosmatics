using MediatR;
using Microsoft.Extensions.Logging;
using MoveLens.Application.Common.Errors;
using MoveLens.Application.Common.Interfaces;
using MoveLens.Application.Features.Identity;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Users.Abstraction;
using MoveLens.Domain.Users.Errors;

namespace MoveLens.Application.Features.Users.Queries.Login;

public sealed class LoginQueryHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IUserRepository userRepository,
    ILogger<LoginQueryHandler> logger)
    : IRequestHandler<LoginQuery, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(LoginQuery query, CancellationToken ct)
    {
        var authResult = await identityService.AuthenticateAsync(query.Email, query.Password, ct);
        if (authResult.IsError)
        {
            logger.LogWarning("Login failed for email {Email}", query.Email);
            return authResult.Errors;
        }

        var user = await userRepository.FindByIdentityIdAsync(authResult.Value.UserId, ct);
        if (user is null)
            return ApplicationErrors.Auth.NotFound(identityId: authResult.Value.UserId);  

        if (!user.IsActive)
            return UserErrors.AccountDeactivated;

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(authResult.Value, ct);
        if (tokenResult.IsError)
        {
            logger.LogError("Token generation failed for user {UserId}", authResult.Value.UserId);
            return ApplicationErrors.Token.GenerationFailed;
        }

        return tokenResult.Value;
    }
}