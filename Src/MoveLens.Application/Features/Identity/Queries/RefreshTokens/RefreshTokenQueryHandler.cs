using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoveLens.Application.Common.Errors;
using MoveLens.Application.Common.Interfaces;
using MoveLens.Application.Features.Identity.Queries.RefreshTokens;
using MoveLens.Domain.Common.Results;
using System.Security.Claims;

namespace MoveLens.Application.Features.Identity.Queries.RefreshToken;

public sealed class RefreshTokenQueryHandler(
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    IAppDbContext context,
    ILogger<RefreshTokenQueryHandler> logger)
    : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery query, CancellationToken ct)
    {
        var principal = tokenProvider.GetPrincipalFromExpiredToken(query.ExpiredAccessToken);
        if (principal is null)
        {
            logger.LogError("Expired access token is not valid");
            return ApplicationErrors.Token.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            logger.LogError("Token does not contain a valid userId claim");
            return ApplicationErrors.Token.UserIdClaimInvalid;
        }

        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == query.RefreshToken && r.UserId == userId, ct);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTimeOffset.UtcNow)
        {
            logger.LogError("Refresh token expired or not found for user {UserId}", userId);
            return ApplicationErrors.Token.RefreshTokenExpired;
        }

        var userResult = await identityService.GetUserByIdAsync(userId, ct);
        if (userResult.IsError)
        {
            logger.LogError("Get user by id failed: {Error}", userResult.TopError.Description);
            return userResult.Errors;
        }

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, ct);
        if (tokenResult.IsError)
        {
            logger.LogError("Token generation failed: {Error}", tokenResult.TopError.Description);
            return ApplicationErrors.Token.GenerationFailed;
        }

        return tokenResult.Value;
    }
}