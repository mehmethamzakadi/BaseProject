using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Attributes;

namespace BaseProject.Domain.Events.CategoryEvents;

[StoreInOutbox]
public class CategoryCreatedEvent : DomainEvent
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public override Guid AggregateId => CategoryId;

    public CategoryCreatedEvent(Guid categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}