namespace MoveLens.Application.Features.Users.Dtos
{

        public sealed record RegisterUserResponse(
            Guid UserId,
            string FullName,
            string IdentityId,
            bool IsActive
        );
    
}
