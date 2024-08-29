namespace Constellation.Core.Models.WorkFlow.Repositories;

using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Training.Identifiers;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICaseRepository
{
    Task<Case?> GetById(CaseId caseId, CancellationToken cancellationToken = default);
    Task<List<Case>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Case>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<bool> ExistingOpenAttendanceCaseForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<Case?> GetOpenAttendanceCaseForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<int> CountActiveActionsForUser(StudentId staffId, CancellationToken cancellationToken = default);
    Task<Case?> GetComplianceCaseForIncident(string incidentId, CancellationToken cancellationToken = default);
    Task<Case?> GetTrainingCaseForStaffAndModule(string staffId, TrainingModuleId moduleId, CancellationToken cancellationToken = default);
    void Insert(Case item);

}
