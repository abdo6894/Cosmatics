using Microsoft.EntityFrameworkCore;
using MoveLens.Domain.Users.Abstraction;
using MoveLens.Domain.Users.Entities;

namespace MoveLens.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> FindByIdAsync(
        Guid id,
        CancellationToken ct = default) =>
        await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> FindByIdentityIdAsync(
        string identityId,
        CancellationToken ct = default) =>
        await context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId, ct);

    public async Task<bool> ExistsByIdentityIdAsync(
        string identityId,
        CancellationToken ct = default) =>
        await context.Users.AnyAsync(u => u.IdentityId == identityId, ct);

    public async Task AddAsync(
     User user,
        CancellationToken ct = default) =>
        await context.Users.AddAsync(user, ct);

    public Task UpdateAsync(
       User user,
        CancellationToken ct = default)
    {
        context.Users.Update(user);
        return Task.CompletedTask;
    }
}
