using MediatR;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Users.Abstraction;
using MoveLens.Domain.Users.Entities;
using MoveLens.Domain.Users.Errors;

namespace MoveLens.Application.Features.Users.Commands.Deactivate;

public sealed class DeactivateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<DeactivateUserCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeactivateUserCommand command, CancellationToken ct)
    {
        var user = await userRepository.FindByIdAsync(command.UserId, ct);
        if (user is null)
            return UserErrors.NotFound(command.UserId);

        var result = user.Deactivate();
        if (result.IsError)
            return result.Errors;

        await userRepository.UpdateAsync(user, ct);
        return Result.Deleted;
    }
}