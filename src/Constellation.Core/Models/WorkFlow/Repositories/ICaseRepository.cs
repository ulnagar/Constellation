namespace Constellation.Core.Models.WorkFlow.Repositories;

using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICaseRepository
{
    Task<Case?> GetById(CaseId caseId, CancellationToken cancellationToken = default);
    Task<List<Case>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Case>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<bool> ExistingOpenAttendanceCaseForStudent(string studentId, CancellationToken cancellationToken = default);
    Task<Case?> GetOpenAttendanceCaseForStudent(string studentId, CancellationToken cancellationToken = default);
    Task<int> CountActiveActionsForUser(string staffId, CancellationToken cancellationToken = default);
    void Insert(Case item);

}
