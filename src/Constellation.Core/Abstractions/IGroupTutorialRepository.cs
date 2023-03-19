#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IGroupTutorialRepository
{
    Task<List<GroupTutorial>> GetAllWithTeachersAndStudents(CancellationToken cancellationToken = default);
    Task<List<GroupTutorial>> GetAllWithTeachersAndStudentsWhereAccessExpired(CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWholeAggregate(GroupTutorialId id, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetById(GroupTutorialId id, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWithTeachersById(GroupTutorialId guid, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWithStudentsById(GroupTutorialId guid, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWithRollsById(GroupTutorialId id, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWithTeachersAndStudentsByName(string name, CancellationToken cancellationToken = default);

    void Insert(GroupTutorial tutorial);
}
