
using MoveLens.Domain.Users.Enums;

namespace MoveLens.Application.Features.Users.Dtos
{

        public sealed record UserPreferencesResponse(
            List<OutingMood> PreferredMoods,
            PreferredLanguage Language,
            decimal? MaxBudget,
            List<string> FavoriteGovernorates
        );
    
}
