namespace MoveLens.Application.Features.Users.Dtos
{

        public sealed record RegisterUserRequest(
            string FullName,
            string Email,
            string Password
        );
    
}
