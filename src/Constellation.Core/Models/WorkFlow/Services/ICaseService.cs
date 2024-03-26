namespace Constellation.Core.Models.WorkFlow.Services;

using Attendance.Identifiers;
using Shared;
using System.Threading;
using System.Threading.Tasks;

public interface ICaseService
{
    // Enter Incident Number to CreateSentralEntryAction => Create new action for confirmation

    Task<Result<Case>> CreateAttendanceCase(string studentId, AttendanceValueId attendanceValueId, CancellationToken cancellationToken = default);
}
