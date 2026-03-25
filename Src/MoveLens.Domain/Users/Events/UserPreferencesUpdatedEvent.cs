using MoveLens.Domain.Common;

namespace MoveLens.Domain.Events;

public sealed class UserPreferencesUpdatedEvent(Guid userId) : DomainEvent
{
    public Guid UserId { get; } = userId;
}
