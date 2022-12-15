namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;

public interface IGroupTutorialRepository
{
    Task<GroupTutorial?> GetWholeAggregate(Guid id, CancellationToken cancellationToken = default);
    void Insert(GroupTutorial tutorial);
    void Remove(GroupTutorial tutorial);
}
