using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Attributes;

namespace BaseProject.Domain.Events.UserEvents;

[StoreInOutbox]
public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string Email { get; }
    public override Guid AggregateId => UserId;

    public UserCreatedEvent(Guid userId, string userName, string email)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
    }
}