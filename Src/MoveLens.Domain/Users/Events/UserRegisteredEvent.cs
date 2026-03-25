using MoveLens.Domain.Common;

namespace MoveLens.Domain.Events;

public sealed class UserRegisteredEvent(Guid userId) : DomainEvent
{
    public Guid UserId { get; } = userId;
}
