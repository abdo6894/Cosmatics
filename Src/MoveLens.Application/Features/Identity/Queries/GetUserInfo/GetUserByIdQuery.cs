

using MediatR;
using MoveLens.Application.Features.Identity.Dtos;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Features.Identity.Queries.GetUserInfo;

public sealed record GetUserByIdQuery(string? UserId) : IRequest<Result<AppUserDto>>;