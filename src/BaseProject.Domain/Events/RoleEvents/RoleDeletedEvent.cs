using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Attributes;

namespace BaseProject.Domain.Events.RoleEvents;

[StoreInOutbox]
public class RoleDeletedEvent : DomainEvent
{
    public Guid RoleId { get; }
    public string RoleName { get; }
    public override Guid AggregateId => RoleId;

    public RoleDeletedEvent(Guid roleId, string roleName)
    {
        RoleId = roleId;
        RoleName = roleName;
    }
}