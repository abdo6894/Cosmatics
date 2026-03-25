
using MoveLens.Domain.Users.Enums;

namespace MoveLens.Domain.Users.ValueObjects;

public sealed class UserPreferences
{
  

    public IReadOnlyList<OutingMood> PreferredMoods { get; private set; } = [];
    public PreferredLanguage Language { get; private set; } = PreferredLanguage.Arabic;
    public decimal? MaxBudget { get; private set; }
    public IReadOnlyList<string> FavoriteGovernorates { get; private set; } = [];


    private UserPreferences() { }


    private UserPreferences(
        List<OutingMood> moods,
        PreferredLanguage language,
        decimal? maxBudget,
        List<string> governorates)
    {
        PreferredMoods = moods.AsReadOnly();
        Language = language;
        MaxBudget = maxBudget;
        FavoriteGovernorates = governorates.AsReadOnly();
    }


    public static UserPreferences Default =>
        new([], PreferredLanguage.Arabic, null, []);

    public static UserPreferences Create(
        List<OutingMood> moods,
        PreferredLanguage language,
        decimal? maxBudget,
        List<string> governorates)
    {
        if (maxBudget is < 0)
            throw new ArgumentException("Budget cannot be negative.", nameof(maxBudget));

        return new(moods, language, maxBudget, governorates);
    }
}