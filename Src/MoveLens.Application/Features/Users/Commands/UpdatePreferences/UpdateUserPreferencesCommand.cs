using MediatR;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Users.Enums;

namespace MoveLens.Application.Features.Users.Commands.UpdatePreferences;

public sealed record UpdateUserPreferencesCommand(
    Guid UserId,
    List<OutingMood> PreferredMoods,
    PreferredLanguage Language,
    decimal? MaxBudget,
    List<string> FavoriteGovernorates
) : IRequest<Result<Updated>>;