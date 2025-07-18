﻿namespace Constellation.Core.Models.WorkFlow.Services;

using Attendance.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Training.Identifiers;
using Shared;
using StaffMembers.Identifiers;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ICaseService
{
    // Enter Incident Number to CreateSentralEntryAction => Create new action for confirmation

    Task<Result<Case>> CreateAttendanceCase(StudentId studentId, AttendanceValueId attendanceValueId, CancellationToken cancellationToken = default);

    Task<Result<Case>> CreateComplianceCase(StudentId studentId, StaffId teacherId, string incidentId, string incidentType, string subject, DateOnly createdDate, CancellationToken cancellationToken = default);

    Task<Result<Case>> CreateTrainingCase(StaffId staffId, TrainingModuleId moduleId, TrainingCompletionId? completionId = null, CancellationToken cancellationToken = default);
}
