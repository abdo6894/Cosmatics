using MediatR;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Users.Abstraction;
using MoveLens.Domain.Users.Errors;
using MoveLens.Domain.Users.ValueObjects;

namespace MoveLens.Application.Features.Users.Commands.UpdatePreferences;

public sealed class UpdateUserPreferencesCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateUserPreferencesCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateUserPreferencesCommand command, CancellationToken ct)
    {
        var user = await userRepository.FindByIdAsync(command.UserId, ct);
        if (user is null)
            return UserErrors.NotFound(command.UserId);

        var preferences = UserPreferences.Create(
            command.PreferredMoods,
            command.Language,
            command.MaxBudget,
            command.FavoriteGovernorates);

        var result = user.UpdatePreferences(preferences);
        if (result.IsError)
            return result.Errors;

        await userRepository.UpdateAsync(user, ct);
        return Result.Updated;
    }
}