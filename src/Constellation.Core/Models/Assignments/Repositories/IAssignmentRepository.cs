﻿#nullable enable

using Constellation;

namespace Constellation.Core.Models.Assignments.Repositories;

using Assignments;
using Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAssignmentRepository
{
    Task<CanvasAssignment?> GetByCanvasId(int CanvasAssignmentId, CancellationToken cancellationToken = default);
    Task<CanvasAssignment?> GetById(AssignmentId id, CancellationToken cancellationToken = default);
    Task<bool> IsValidAssignmentId(AssignmentId id, CancellationToken cancellationToken = default);
    Task<List<CanvasAssignment>> GetByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<CanvasAssignment>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<List<CanvasAssignment>> GetAllCurrentAndFuture(CancellationToken cancellationToken = default);
    Task<List<CanvasAssignment>> GetAllDueForUpload(CancellationToken cancellationToken = default);
    Task<List<CanvasAssignment>> GetForCleanup(CancellationToken cancellationToken = default);
    void Insert(CanvasAssignment entity);
}
