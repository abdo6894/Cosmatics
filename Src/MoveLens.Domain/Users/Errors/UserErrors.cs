using MoveLens.Domain.Common.Results;

namespace MoveLens.Domain.Users.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");

    public static Error AlreadyExists =>
        Error.Conflict("User.AlreadyExists", "A user with this identity already exists.");

    public static Error AccountDeactivated =>
        Error.Forbidden("User.Deactivated", "This account has been deactivated.");

    public static Error CannotUpdateDeactivated =>
        Error.Failure("User.CannotUpdate", "Cannot update a deactivated account.");
}