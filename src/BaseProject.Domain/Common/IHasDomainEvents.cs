namespace BaseProject.Domain.Common;

/// <summary>
/// Domain event'leri destekleyen entity'ler için interface
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent eventItem);
    void RemoveDomainEvent(IDomainEvent eventItem);
    void ClearDomainEvents();
}
