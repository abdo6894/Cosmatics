using MoveLens.Domain.Common;

namespace MoveLens.Domain.Events;

public sealed class UserProfileUpdatedEvent(Guid userId, string newFullName) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string NewFullName { get; } = newFullName;
}
