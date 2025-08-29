namespace Constellation.Core.Models.WorkFlow.Repositories;

using Abstractions.Clock;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Training.Identifiers;
using Enums;
using Identifiers;
using StaffMembers.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICaseRepository
{
    Task<Case?> GetById(CaseId caseId, CancellationToken cancellationToken = default);
    Task<List<Case>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Case>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<List<Case>> GetForStaffMember(StaffId staffId, CancellationToken cancellationToken = default);
    Task<List<Case>> GetFilteredCases(StaffId staffId, CaseStatusFilter filter, IDateTimeProvider dateTime, CancellationToken cancellationToken = default);
    Task<bool> ExistingOpenAttendanceCaseForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<Case?> GetOpenAttendanceCaseForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<int> CountActiveActionsForUser(StaffId staffId, CancellationToken cancellationToken = default);
    Task<Case?> GetComplianceCaseForIncident(string incidentId, CancellationToken cancellationToken = default);
    Task<Case?> GetTrainingCaseForStaffAndModule(StaffId staffId, TrainingModuleId moduleId, CancellationToken cancellationToken = default);
    void Insert(Case item);

}
