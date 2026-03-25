using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class Token
    {
        public static Error ExpiredAccessTokenInvalid =>
            Error.Unauthorized("Token.ExpiredAccessTokenInvalid", "Expired access token is not valid.");

        public static Error UserIdClaimInvalid =>
            Error.Unauthorized("Token.UserIdClaimInvalid", "Token does not contain a valid user identifier.");

        public static Error RefreshTokenExpired =>
            Error.Unauthorized("Token.RefreshTokenExpired", "Refresh token has expired or does not exist.");

        public static Error GenerationFailed =>
            Error.Unexpected("Token.GenerationFailed", "An error occurred while generating the token.");
    }

    public static class Auth
    {
        public static Error InvalidCredentials =>
            Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect.");

        public static Error AccountLockedOut =>
            Error.Forbidden("Auth.AccountLockedOut", "Account is locked. Please try again later.");

        public static Error EmailAlreadyExists =>
            Error.Conflict("Auth.EmailAlreadyExists", "An account with this email already exists.");

        public static Error UserNotFound =>
            Error.NotFound("Auth.UserNotFound", "User not found.");
        public static Error NotFound(string identityId) =>
            Error.NotFound("User.NotFound", $"User with identity id '{identityId}' was not found.");
    }
}