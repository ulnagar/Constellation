#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IGroupTutorialRepository
{
    Task<List<GroupTutorial>> GetAllWithTeachersAndStudents(CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWholeAggregate(Guid id, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWithTeachersById(Guid guid, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetWithStudentsById(Guid guid, CancellationToken cancellationToken = default);

    void Insert(GroupTutorial tutorial);
}
