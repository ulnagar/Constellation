namespace Constellation.Core.Models.WorkFlow.Repositories;

using Constellation.Core.Models.WorkFlow.Identifiers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

public interface ICaseRepository
{
    Task<Case> GetById(CaseId caseId, CancellationToken cancellationToken = default);
    Task<List<Case>> GetAll(CancellationToken cancellationToken = default);
    Task<bool> ExistingOpenAttendanceCaseForStudent(string studentId, CancellationToken cancellationToken = default);
    Task<Case?> GetOpenAttendanceCaseForStudent(string studentId, CancellationToken cancellationToken = default);
    void Insert(Case item);

}
