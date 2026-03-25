

using MediatR;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Features.Identity.Queries.RefreshTokens;

public record RefreshTokenQuery(string RefreshToken, string ExpiredAccessToken) : IRequest<Result<TokenResponse>>;