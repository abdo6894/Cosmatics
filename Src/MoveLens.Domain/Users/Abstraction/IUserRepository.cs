using MoveLens.Domain.Users.Entities;

namespace MoveLens.Domain.Users.Abstraction;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);

    Task<User?> FindByIdentityIdAsync(string identityId, CancellationToken ct = default);

    Task<bool> ExistsByIdentityIdAsync(string identityId, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    Task UpdateAsync(User user, CancellationToken ct = default);
}