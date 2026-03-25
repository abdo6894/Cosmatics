using MediatR;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Users.Abstraction;
using MoveLens.Domain.Users.Errors;

namespace MoveLens.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateUserProfileCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateUserProfileCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateUserProfileCommand command, CancellationToken ct)
    {
        var user = await userRepository.FindByIdAsync(command.UserId, ct);
        if (user is null)
            return UserErrors.NotFound(command.UserId);

        var result = user.UpdateProfile(command.FullName);
        if (result.IsError)
            return result.Errors;

        await userRepository.UpdateAsync(user, ct);
        return Result.Updated;
    }
}