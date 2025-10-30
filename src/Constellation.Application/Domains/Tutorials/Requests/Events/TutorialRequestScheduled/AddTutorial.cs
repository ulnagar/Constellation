namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestScheduled;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;
using Constellation.Application.Domains.Tutorials.Requests.Commands.ScheduleTutorialRequest;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Timetables.Identifiers;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Enums;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Enrolments;
using Core.Models.Tutorials.Events;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class AddTutorial
: IDomainEventHandler<TutorialRequestScheduledDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _sentralGateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTutorial(
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        ISentralGateway sentralGateway,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _sentralGateway = sentralGateway;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TutorialRequestScheduledDomainEvent notification, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to create Tutorial for Tutorial Request");

            return;
        }

        if (tutorialRequest.Plan is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.PlanNotFound(notification.RequestId), true)
                .Warning("Failed to create Tutorial for Tutorial Request");

            return;
        }

        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to create Tutorial for Tutorial Request");

            return;
        }

        List<ValidAttendenceReportDate> weekDescriptors = await _sentralGateway.GetTermsAndWeeksFromApi(_dateTime.CurrentYearAsString, cancellationToken);

        ValidAttendenceReportDate startWeek = weekDescriptors.FirstOrDefault(entry =>
            entry.StartDate == tutorialRequest.Plan.StartDate.ToDateTime(TimeOnly.MinValue));

        int index = weekDescriptors.IndexOf(startWeek);

        int endWeekIndex = index + 10;

        ValidAttendenceReportDate endWeek = (endWeekIndex > weekDescriptors.Count - 1)
            ? weekDescriptors.Last()
            : weekDescriptors[endWeekIndex];

        // Does tutorial already exist?
        Tutorial tutorial = await _tutorialRepository.GetByNameAndYear(_dateTime.CurrentYear, tutorialRequest.Plan.Name, cancellationToken);

        if (tutorial is not null)
        {
            if (tutorial.EndDate < tutorialRequest.Plan.StartDate)
            {
                tutorial.Update(
                    tutorial.Name, 
                    tutorialRequest.Plan.StartDate, 
                    DateOnly.FromDateTime(endWeek.EndDate), 
                    _dateTime);
            }
        }
        else
        {
            Result<Tutorial> newTutorial = Tutorial.Create(
                tutorialRequest.Plan.Name,
                tutorialRequest.Plan.StartDate,
                DateOnly.FromDateTime(endWeek.EndDate),
                _dateTime);

            if (newTutorial.IsFailure)
            {
                _logger
                    .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                    .ForContext(nameof(Error), newTutorial.Error, true)
                    .Warning("Failed to create Tutorial for Tutorial Request");

                return;
            }

            tutorial = newTutorial.Value;

            _tutorialRepository.Insert(tutorial);
        }

        tutorialRequest.Plan.Update(tutorial.Id);

        // Add sessions to new tutorial

        foreach ((PeriodId PeriodId, StaffId StaffId) session in tutorialRequest.Plan.Periods)
            tutorial.AddSession(session.PeriodId, session.StaffId);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
