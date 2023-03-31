#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAssignmentRepository
{

    Task<CanvasAssignment?> GetById(AssignmentId id, CancellationToken cancellationToken = default);
    Task<bool> IsValidAssignmentId(AssignmentId id, CancellationToken cancellationToken = default);
    Task<List<CanvasAssignment>> GetByCourseId(int courseId, CancellationToken cancellationToken = default);
    Task<List<CanvasAssignment>> GetAllCurrent(CancellationToken cancellationToken = default);
    void Insert(CanvasAssignment entity);
}
