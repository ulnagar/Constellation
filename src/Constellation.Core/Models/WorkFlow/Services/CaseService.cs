using Constellation.Core.Models.Students.Identifiers;

namespace Constellation.Core.Models.WorkFlow.Services;

using Abstractions.Clock;
using Abstractions.Services;
using Attendance;
using Attendance.Errors;
using Attendance.Repositories;
using Constellation.Core.Models.Attendance.Identifiers;
using Constellation.Core.Models.Training.Errors;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Models.Training.Repositories;
using Core.Errors;
using Enums;
using Shared;
using StaffMembers.Repositories;
using Students;
using Students.Errors;
using Students.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Training;
using WorkFlow;

public sealed class CaseService : ICaseService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;

    public CaseService(
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository,
        IStaffRepository staffRepository,
        ITrainingModuleRepository moduleRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime)
    {
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
        _staffRepository = staffRepository;
        _moduleRepository = moduleRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<Case>> CreateAttendanceCase(
        StudentId studentId, 
        AttendanceValueId attendanceValueId,
        CancellationToken cancellationToken = default)
    {
        Staff? currentUser = await _staffRepository.GetCurrentByEmailAddress(_currentUserService.EmailAddress, cancellationToken);

        if (currentUser is null)
            return Result.Failure<Case>(DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress));

        AttendanceValue? value = await _attendanceRepository.GetById(attendanceValueId, cancellationToken);

        if (value is null)
            return Result.Failure<Case>(AttendanceValueErrors.NotFound(attendanceValueId));

        Student? student = await _studentRepository.GetById(value.StudentId, cancellationToken);

        if (student is null)
            return Result.Failure<Case>(StudentErrors.NotFound(value.StudentId));

        // Create Case
        Case item = new(CaseType.Attendance);

        // Create and add CaseDetail
        Result<CaseDetail> detail = AttendanceCaseDetail.Create(
            student,
            value);

        if (detail.IsFailure)
            return Result.Failure<Case>(detail.Error);

        Result attach = item.AttachDetails(detail.Value);

        if (attach.IsFailure)
            return Result.Failure<Case>(attach.Error);

        Result dueDate = item.SetDueDate(_dateTime);

        if (dueDate.IsFailure)
            return Result.Failure<Case>(dueDate.Error);

        return item;
    }

    public async Task<Result<Case>> CreateComplianceCase(
        StudentId studentId,
        string teacherId,
        string incidentId,
        string incidentType,
        string subject,
        DateOnly createdDate,
        CancellationToken cancellationToken = default)
    {
        Staff? teacher = await _staffRepository.GetById(teacherId, cancellationToken);

        if (teacher is null)
            return Result.Failure<Case>(DomainErrors.Partners.Staff.NotFound(teacherId));

        Student? student = await _studentRepository.GetById(studentId, cancellationToken);

        if (student is null)
            return Result.Failure<Case>(StudentErrors.NotFound(studentId));

        if (string.IsNullOrWhiteSpace(incidentId))
            return Result.Failure<Case>(ApplicationErrors.ArgumentNull(nameof(incidentId)));

        if (string.IsNullOrWhiteSpace(incidentType))
            return Result.Failure<Case>(ApplicationErrors.ArgumentNull(nameof(incidentType)));

        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure<Case>(ApplicationErrors.ArgumentNull(nameof(subject)));

        // Create Case
        Case item = new(CaseType.Compliance);

        // Create and add CaseDetail
        Result<CaseDetail> detail = ComplianceCaseDetail.Create(
            student,
            teacher,
            incidentId,
            incidentType,
            subject,
            createdDate);

        if (detail.IsFailure)
            return Result.Failure<Case>(detail.Error);

        Result attach = item.AttachDetails(detail.Value);

        if (attach.IsFailure)
            return Result.Failure<Case>(attach.Error);

        Result dueDate = item.SetDueDate(_dateTime);

        if (dueDate.IsFailure)
            return Result.Failure<Case>(dueDate.Error);

        return item;
    }

    public async Task<Result<Case>> CreateTrainingCase(
        string staffId,
        TrainingModuleId moduleId,
        TrainingCompletionId? completionId = null,
        CancellationToken cancellationToken = default)
    {
        Staff? teacher = await _staffRepository.GetById(staffId, cancellationToken);

        if (teacher is null)
            return Result.Failure<Case>(DomainErrors.Partners.Staff.NotFound(staffId));

        TrainingModule module = await _moduleRepository.GetModuleById(moduleId, cancellationToken);

        if (module is null)
            return Result.Failure<Case>(TrainingModuleErrors.NotFound(moduleId));

        TrainingCompletion completion = completionId != null
            ? module.Completions.FirstOrDefault(completion => completion.Id == completionId)
            : null;

        Case item = new(CaseType.Training);

        Result<CaseDetail> detail = TrainingCaseDetail.Create(
            teacher,
            module,
            completion,
            _dateTime);

        if (detail.IsFailure)
            return Result.Failure<Case>(detail.Error);

        Result attach = item.AttachDetails(detail.Value);

        if (attach.IsFailure)
            return Result.Failure<Case>(attach.Error);

        DateOnly dueDate = ((TrainingCaseDetail)detail.Value).DaysUntilDue < 0
            ? _dateTime.Today
            : ((TrainingCaseDetail)detail.Value).DueDate;

        Result setDueDate = item.SetDueDate(_dateTime, dueDate);

        if (setDueDate.IsFailure)
            return Result.Failure<Case>(setDueDate.Error);

        return item;
    }
}
