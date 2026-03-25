using MediatR;
using MoveLens.Application.Features.Identity;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Features.Users.Queries.Login;

public sealed record LoginQuery(
    string Email,
    string Password
) : IRequest<Result<TokenResponse>>;