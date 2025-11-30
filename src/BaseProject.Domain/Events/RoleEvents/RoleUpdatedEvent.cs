using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Attributes;

namespace BaseProject.Domain.Events.RoleEvents;

[StoreInOutbox]
public class RoleUpdatedEvent : DomainEvent
{
    public Guid RoleId { get; }
    public string RoleName { get; }
    public override Guid AggregateId => RoleId;

    public RoleUpdatedEvent(Guid roleId, string roleName)
    {
        RoleId = roleId;
        RoleName = roleName;
    }
}