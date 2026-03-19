namespace Constellation.Core.Models.Attendance.Repositories;

using Core.Enums;
using Identifiers;
using Models.Identifiers;
using Students.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAttendancePlanRepository
{
    Task<AttendancePlan?> GetById(AttendancePlanId id, CancellationToken cancellationToken = default);
    Task<List<AttendancePlan>> GetAll(CancellationToken cancellationToken = default);
    Task<List<AttendancePlan>> GetPendingForSchool(SchoolCode schoolCode, CancellationToken cancellationToken = default);
    Task<List<AttendancePlan>> GetForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<AttendancePlan?> GetCurrentApprovedForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<AttendancePlan>> GetForSchool(SchoolCode schoolCode, CancellationToken cancellationToken = default);
    Task<List<AttendancePlan>> GetRecentForSchoolAndGrade(SchoolCode schoolCode, Grade grade, CancellationToken cancellationToken = default);
    Task<int> GetCountOfPending(CancellationToken cancellationToken = default);
    Task<int> GetCountOfProcessing(CancellationToken cancellationToken = default);
    void Insert(AttendancePlan plan);
}