namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialsForStudent;

using Abstractions.Messaging;
using Attendance.Reports.Queries.GetValidAttendanceReportDates;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Gateways;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialsForStudentQueryHandler
: IQueryHandler<GetTutorialsForStudentQuery, List<TutorialResponse>>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISentralGateway _sentralGateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetTutorialsForStudentQueryHandler(
        ITutorialRepository tutorialRepository,
        IPeriodRepository periodRepository,
        IStaffRepository staffRepository,
        ISentralGateway sentralGateway,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _periodRepository = periodRepository;
        _staffRepository = staffRepository;
        _sentralGateway = sentralGateway;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetTutorialsForStudentQuery>();
    }

    public async Task<Result<List<TutorialResponse>>> Handle(GetTutorialsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<TutorialResponse> responses = [];

        List<ValidAttendenceReportDate> weekDescriptors = await _sentralGateway.GetTermsAndWeeksFromApi(_dateTime.CurrentYearAsString, cancellationToken);

        List<Tutorial> tutorials = await _tutorialRepository.GetAllForStudent(request.StudentId, cancellationToken);

        foreach (Tutorial tutorial in tutorials)
        {
            ValidAttendenceReportDate startWeek = weekDescriptors.FirstOrDefault(entry =>
                entry.StartDate <= tutorial.StartDate.ToDateTime(TimeOnly.MinValue) &&
                entry.EndDate >= tutorial.StartDate.ToDateTime(TimeOnly.MinValue));

            ValidAttendenceReportDate endWeek = weekDescriptors.FirstOrDefault(entry =>
                entry.StartDate <= tutorial.EndDate.ToDateTime(TimeOnly.MinValue) &&
                entry.EndDate >= tutorial.EndDate.ToDateTime(TimeOnly.MinValue));

            if (startWeek is null || endWeek is null)
            {
                _logger
                    .ForContext(nameof(Tutorial), tutorial, true)
                    .Warning("Failed to determine start or end week descriptor");

                continue;
            }

            List<StaffId> teacherIds = tutorial.Sessions
                .Where(entry => !entry.IsDeleted)
                .Select(entry => entry.StaffId)
                .Distinct()
                .ToList();

            List<StaffMember> teachers = await _staffRepository.GetListFromIds(teacherIds, cancellationToken);

            List<PeriodId> periodIds = tutorial.Sessions
                .Where(entry => !entry.IsDeleted)
                .Select(entry => entry.PeriodId)
                .Distinct()
                .ToList();

            List<Period> periods = await _periodRepository.GetListFromIds(periodIds, cancellationToken);

            TutorialResponse response = new(
                tutorial.Id,
                tutorial.Name,
                startWeek.Description,
                endWeek.Description,
                teachers.Select(entry => entry.Name).ToList(),
                periods);

            responses.Add(response);
        }

        return responses;
    }
}
