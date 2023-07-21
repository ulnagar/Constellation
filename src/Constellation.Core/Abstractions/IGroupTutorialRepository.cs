#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IGroupTutorialRepository
{
    Task<GroupTutorial?> GetById(GroupTutorialId id, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetByName(string name, CancellationToken cancellationToken = default);
    Task<List<GroupTutorial>> GetAll(CancellationToken cancellationToken = default);
    Task<List<GroupTutorial>> GetAllWhereAccessExpired(CancellationToken cancellationToken = default);

    void Insert(GroupTutorial tutorial);
}
