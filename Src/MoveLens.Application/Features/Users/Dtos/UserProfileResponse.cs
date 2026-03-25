using System;
using System.Collections.Generic;
using System.Text;

namespace MoveLens.Application.Features.Users.Dtos
{


        public sealed record UserProfileResponse(
            Guid UserId,
            string FullName,
            string IdentityId,
            bool IsActive,
            UserPreferencesResponse Preferences
        );
    
}
