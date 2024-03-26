namespace Constellation.Core.Models.WorkFlow.Services;

using Attendance.Repositories;
using Constellation.Core.Models.Attendance.Identifiers;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Repositories;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

public sealed class CaseService : ICaseService
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public CaseService(
        ICaseRepository caseRepository,
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository)
    {
        _caseRepository = caseRepository;
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<Result<Case>> CreateAttendanceCase(
        string studentId, 
        AttendanceValueId attendanceValueId,
        CancellationToken cancellationToken = default)
    {
        Case item = new Case(CaseType.Attendance);

        
    }
}
