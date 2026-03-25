using MoveLens.Application.Features.Identity.Dtos;
using MoveLens.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoveLens.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<Result<AppUserDto>> CreateAsync(string email, string password, CancellationToken ct = default);

        Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct = default);

        Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken ct = default);

        Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct = default);

        Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default);
    }
}
