using MoveLens.Domain.Common;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Events;
using MoveLens.Domain.Users.Errors;
using MoveLens.Domain.Users.ValueObjects;

namespace MoveLens.Domain.Users.Entities;

    public sealed class User : AuditableEntity
    {
        public string FullName { get; private set; } = default!;
        public string IdentityId { get; private set; } = default!;
        public UserPreferences Preferences { get; private set; } = default!;
        public bool IsActive { get; private set; }

        private User() { }

        private User(Guid id, string fullName, string identityId, UserPreferences preferences)
            : base(id)
        {
            FullName = fullName;
            IdentityId = identityId;
            Preferences = preferences;
            IsActive = true;
        }


        public static Result<User> Create(string? fullName, string? identityId, UserPreferences? preferences)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Error.Validation("User.InvalidFullName", "Full name cannot be empty.");

            if (fullName.Length > 100)
                return Error.Validation("User.FullNameTooLong", "Full name must not exceed 100 characters.");

            if (string.IsNullOrWhiteSpace(identityId))
                return Error.Validation("User.InvalidIdentityId", "Identity id cannot be empty.");

            var user = new User(
                Guid.NewGuid(),
                fullName.Trim(),
                identityId,
                preferences ?? UserPreferences.Default);

            user.AddDomainEvent(new UserRegisteredEvent(user.Id));

            return user;
        }


        public Result<Updated> UpdateProfile(string? fullName)
        {
            if (!IsActive)
                return UserErrors.CannotUpdateDeactivated;

            if (string.IsNullOrWhiteSpace(fullName))
                return Error.Validation("User.InvalidFullName", "Full name cannot be empty.");

            if (fullName.Length > 100)
                return Error.Validation("User.FullNameTooLong", "Full name must not exceed 100 characters.");

            FullName = fullName.Trim();

            AddDomainEvent(new UserProfileUpdatedEvent(Id, FullName));

            return Result.Updated;
        }

        public Result<Updated> UpdatePreferences(UserPreferences preferences)
        {
            if (!IsActive)
                return UserErrors.CannotUpdateDeactivated;

            Preferences = preferences;

            AddDomainEvent(new UserPreferencesUpdatedEvent(Id));

            return Result.Updated;
        }

        public Result<Deleted> Deactivate()
        {
            if (!IsActive)
                return UserErrors.AccountDeactivated;

            IsActive = false;

            AddDomainEvent(new UserDeactivatedEvent(Id));

            return Result.Deleted;
        }
    }