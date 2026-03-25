using MoveLens.Application.Features.Identity;
using MoveLens.Application.Features.Identity.Dtos;
using MoveLens.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace MoveLens.Application.Common.Interfaces
{
    public interface ITokenProvider
    {
        Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
