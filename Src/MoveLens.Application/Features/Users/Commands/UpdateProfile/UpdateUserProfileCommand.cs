using MediatR;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Features.Users.Commands.UpdateProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FullName
) : IRequest<Result<Updated>>;