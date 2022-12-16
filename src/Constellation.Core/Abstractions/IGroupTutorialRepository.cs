using Constellation.Core.Models.GroupTutorials;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Core.Abstractions;

public interface IGroupTutorialRepository
{
    Task<GroupTutorial> GetWholeAggregate(Guid id, CancellationToken cancellationToken = default);
    void Insert(GroupTutorial tutorial);
}
