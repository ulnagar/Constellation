namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialDetails;

using Abstractions.Messaging;
using Constellation.Core.Models.Timetables;
using Constellation.Core.Models.Timetables.Repositories;
using Core.Abstractions.Services;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialDetailsQueryHandler
: IQueryHandler<GetTutorialDetailsQuery, TutorialDetailsResponse>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public GetTutorialDetailsQueryHandler(
        ITutorialRepository tutorialRepository,
        IStaffRepository staffRepository,
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        IPeriodRepository periodRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _staffRepository = staffRepository;
        _studentRepository = studentRepository;
        _enrolmentRepository = enrolmentRepository;
        _periodRepository = periodRepository;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<GetTutorialDetailsQuery>();
    }

    public async Task<Result<TutorialDetailsResponse>> Handle(GetTutorialDetailsQuery request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.Id, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(GetTutorialDetailsQuery), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Tutorial details for user {User}", _currentUserService.UserName);

            return Result.Failure<TutorialDetailsResponse>(TutorialErrors.NotFound(request.Id));
        }

        List<StaffId> staffIds = tutorial
            .Sessions
            .Where(session => !session.IsDeleted)
            .Select(session => session.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> teachers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByTutorialId(tutorial.Id, cancellationToken);

        List<StudentId> studentIds = enrolments
            .Select(enrolment => enrolment.StudentId)
            .ToList();

        List<Student> activeStudents = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        List<TutorialDetailsResponse.SessionSummary> sessions = [];

        int duration = 0;

        foreach (var session in tutorial.Sessions)
        {
            if (session.IsDeleted)
                continue;

            StaffMember teacher = teachers.FirstOrDefault(teacher => teacher.Id == session.StaffId);

            Period period = await _periodRepository.GetById(session.PeriodId, cancellationToken);

            if (period is null)
            {
                _logger.Warning("Could not find Period with Id {id}", session.PeriodId);

                continue;
            }

            duration += period.Duration;

            sessions.Add(new(
                session.Id,
                period.ToString(),
                period.SortOrder,
                period.Week,
                period.Day,
                period.StartTime,
                period.EndTime,
                period.Duration,
                new(
                    teacher.Id,
                    teacher.EmployeeId,
                    teacher.Name)));
        }

        List<TutorialDetailsResponse.StudentSummary> students = [];

        foreach (var student in activeStudents)
        {
            students.Add(new(
                student.Id,
                student.StudentReferenceNumber,
                student.Gender,
                student.Name,
                student.CurrentEnrolment?.Grade,
                student.CurrentEnrolment?.SchoolCode,
                student.CurrentEnrolment?.SchoolName,
                student.CurrentEnrolment is not null));
        }

        List<TutorialDetailsResponse.ResourceSummary> teams = [];

        foreach (var resource in tutorial.Teams)
        {
            teams.Add(new(
                resource.Id,
                resource.Name,
                resource.Url));
        }

        return new TutorialDetailsResponse(
            tutorial.Id,
            tutorial.Name,
            tutorial.StartDate,
            tutorial.EndDate,
            tutorial.IsCurrent,
            students,
            sessions,
            teams,
            duration);
    }
}