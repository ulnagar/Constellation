namespace Constellation.Core.Primitives;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}