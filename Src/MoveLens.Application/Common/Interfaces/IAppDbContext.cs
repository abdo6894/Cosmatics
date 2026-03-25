using Microsoft.EntityFrameworkCore;
using MoveLens.Domain.Identity;
using MoveLens.Domain.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoveLens.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<RefreshToken> RefreshTokens { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
