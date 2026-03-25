using MediatR;
using MoveLens.Application.Features.Users.Dtos;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Features.Users.Queries.GetProfile;

public sealed record GetUserProfileQuery(string IdentityId) : IRequest<Result<UserProfileResponse>>;