namespace Constellation.Core.Models.WorkFlow.Services;

using Abstractions.Services;
using Attendance;
using Attendance.Errors;
using Attendance.Repositories;
using Constellation.Core.Models.Attendance.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Repositories;
using Constellation.Core.Shared;
using Core.Errors;
using Enrolments;
using Enrolments.Repositories;
using Offerings;
using Offerings.Errors;
using Offerings.Repositories;
using StaffMembers.Repositories;
using Students;
using Students.Errors;
using Students.Repositories;
using Subjects.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class CaseService : ICaseService
{
    private const string _htWellbeingId = "1077017";

    private readonly ICaseRepository _caseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;

    public CaseService(
        ICaseRepository caseRepository,
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository,
        IStaffRepository staffRepository,
        ICurrentUserService currentUserService,
        IEnrolmentRepository enrolmentRepository,
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository)
    {
        _caseRepository = caseRepository;
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
        _staffRepository = staffRepository;
        _currentUserService = currentUserService;
        _enrolmentRepository = enrolmentRepository;
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
    }

    public async Task<Result<Case>> CreateAttendanceCase(
        string studentId, 
        AttendanceValueId attendanceValueId,
        CancellationToken cancellationToken = default)
    {
        Staff currentUser = await _staffRepository.GetCurrentByEmailAddress(_currentUserService.EmailAddress, cancellationToken);

        if (currentUser is null)
            return Result.Failure<Case>(DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress));

        Staff reviewer = await _staffRepository.GetById(_htWellbeingId, cancellationToken);

        if (reviewer is null)
            return Result.Failure<Case>(DomainErrors.Partners.Staff.NotFound(_htWellbeingId));

        AttendanceValue value = await _attendanceRepository.GetById(attendanceValueId, cancellationToken);

        if (value is null)
            return Result.Failure<Case>(AttendanceValueErrors.NotFound(attendanceValueId));

        Student student = await _studentRepository.GetWithSchoolById(value.StudentId, cancellationToken);

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

        //TODO: Should this be moved to Domain Event handlers instead?

        // Create Action 1: Send Email to parents by HT Wellbeing
        Result<SendEmailAction> emailAction = SendEmailAction.Create(item.Id, reviewer, _currentUserService.UserName);

        if (emailAction.IsFailure)
            return Result.Failure<Case>(emailAction.Error);

        item.AddAction(emailAction.Value);

        AttendanceCaseDetail caseDetail = detail.Value as AttendanceCaseDetail;

        if (caseDetail!.Severity.Equals(AttendanceSeverity.BandOne))
            return item;

        if (caseDetail!.Severity.Equals(AttendanceSeverity.BandThree))
        {
            // Create Action : Phone Parents assigned to HT Wellbeing

            Result<PhoneParentAction> phoneAction = PhoneParentAction.Create(item.Id, reviewer, _currentUserService.UserName);

            if (phoneAction.IsFailure)
                return Result.Failure<Case>(phoneAction.Error);

            item.AddAction(phoneAction.Value);
        }

        if (caseDetail!.Severity.Equals(AttendanceSeverity.BandFour))
        {
            // Create Action : Parent Interview assigned to HT Wellbeing

            Result<ParentInterviewAction> interviewAction = ParentInterviewAction.Create(item.Id, reviewer, _currentUserService.UserName);

            if (interviewAction.IsFailure)
                return Result.Failure<Case>(interviewAction.Error);

            item.AddAction(interviewAction.Value);
        }

        // Create Action : Create Sentral Incident for each enrolled class
        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(studentId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            if (offering is null)
                return Result.Failure<Case>(OfferingErrors.NotFound(enrolment.OfferingId));

            if (offering.Sessions.Count == 0)
                continue;

            List<string> teacherIds = offering.Teachers
                .Where(entry => 
                    entry.Type == AssignmentType.ClassroomTeacher &&
                    !entry.IsDeleted)
                .Select(entry => entry.StaffId)
                .ToList();

            foreach (string teacherId in teacherIds)
            {
                Staff teacher = await _staffRepository.GetById(teacherId, cancellationToken);

                if (teacher is null) 
                    continue;

                Result<CreateSentralEntryAction> action = CreateSentralEntryAction.Create(item.Id, teacher, offering, _currentUserService.UserName);

                if (action.IsFailure)
                    return Result.Failure<Case>(action.Error);

                item.AddAction(action.Value);
            }
        }

        return item;
    }
}
