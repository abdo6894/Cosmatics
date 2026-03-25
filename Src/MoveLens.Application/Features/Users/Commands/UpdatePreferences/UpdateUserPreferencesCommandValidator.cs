using FluentValidation;

namespace MoveLens.Application.Features.Users.Commands.UpdatePreferences;

public sealed class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
{
    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleForEach(x => x.PreferredMoods)
            .IsInEnum().WithMessage("Invalid outing mood value.");

        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("Invalid language value.");

        RuleFor(x => x.MaxBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget cannot be negative.")
            .When(x => x.MaxBudget.HasValue);
    }
}