namespace Constellation.Application.Domains.Tutorials.Requests.Commands.ScheduleTutorialRequest;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;
using Constellation.Application.Helpers;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Tutorials.Enums;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Extensions;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ScheduleTutorialRequestCommandHandler
: ICommandHandler<ScheduleTutorialRequestCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISentralGateway _gateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ScheduleTutorialRequestCommandHandler(
        IStudentRepository studentRepository,
        ITutorialRepository tutorialRepository,
        IMSTeamOperationsRepository operationsRepository,
        ICurrentUserService currentUserService,
        ISentralGateway gateway,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _tutorialRepository = tutorialRepository;
        _operationsRepository = operationsRepository;
        _currentUserService = currentUserService;
        _gateway = gateway;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ScheduleTutorialRequestCommand>();
    }

    public async Task<Result> Handle(ScheduleTutorialRequestCommand request, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(request.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(request.RequestId), true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(TutorialRequestErrors.NotFound(request.RequestId));
        }

        Result result = tutorialRequest.Review(RequestStatus.Scheduled, request.Comment, _currentUserService.UserName, _dateTime);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(result.Error);
        }

        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(StudentErrors.NotFound(tutorialRequest.StudentId));
        }

        List<ValidAttendenceReportDate> weekDescriptors = await _gateway.GetTermsAndWeeksFromApi(_dateTime.CurrentYearAsString, cancellationToken);

        ValidAttendenceReportDate startWeek = weekDescriptors.FirstOrDefault(entry =>
            entry.StartDate == request.StartDate.ToDateTime(TimeOnly.MinValue));

        int index = weekDescriptors.IndexOf(startWeek);

        int endWeekIndex = index + 10;

        ValidAttendenceReportDate endWeek = (endWeekIndex > weekDescriptors.Count - 1)
            ? weekDescriptors.Last()
            : weekDescriptors[endWeekIndex];

        Result<Tutorial> tutorial = Tutorial.Create(
            request.Name,
            request.StartDate,
            DateOnly.FromDateTime(endWeek.EndDate),
            _dateTime);

        if (tutorial.IsFailure)
        {
            _logger
                .ForContext(nameof(ScheduleTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), tutorial.Error, true)
                .Warning("Failed to schedule Tutorial Request");

            return Result.Failure(tutorial.Error);
        }

        _tutorialRepository.Insert(tutorial.Value);

        foreach ((PeriodId PeriodId, StaffId StaffId) session in request.Periods)
            tutorial.Value.AddSession(session.PeriodId, session.StaffId);

        TutorialCreatedMSTeamOperation operation = new()
        {
            DateScheduled = DateTime.Now,
            TeamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Value.Name),
            Action = MSTeamOperationAction.Add,
            TutorialId = tutorial.Value.Id,
            TeamDescription = $"8912;TUT;Support;{_dateTime.CurrentYearAsString};{tutorialRequest.Grade.AsName()};"
        };

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
