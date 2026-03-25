using AutoMapper;
using MediatR;
using MoveLens.Application.Common.Errors;
using MoveLens.Application.Features.Users.Dtos;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Users.Abstraction;

namespace MoveLens.Application.Features.Users.Queries.GetProfile;

public sealed class GetUserProfileQueryHandler(
    IUserRepository userRepository,
    IMapper mapper)
    : IRequestHandler<GetUserProfileQuery, Result<UserProfileResponse>>
{
    public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        var user = await userRepository.FindByIdentityIdAsync(query.IdentityId, ct);
        if (user is null)
            return ApplicationErrors.Auth.NotFound(query.IdentityId);

        return mapper.Map<UserProfileResponse>(user);
    }
}