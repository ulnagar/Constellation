namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestScheduled;

using Abstractions.Messaging;
using Constellation.Application.Domains.Tutorials.Requests.Commands.ScheduleTutorialRequest;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Operations.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Repositories;
using Core.Abstractions.Clock;
using Core.Extensions;
using Core.Models.Tutorials.Events;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTeam
: IDomainEventHandler<TutorialRequestScheduledDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamOperationRepository _teamOperationRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateTeam(
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        ITeamOperationRepository teamOperationRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _teamOperationRepository = teamOperationRepository;
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
                .Warning("Failed to create Team for Tutorial Request");

            return;
        }

        if (tutorialRequest.Plan is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.PlanNotFound(notification.RequestId), true)
                .Warning("Failed to create Team for Tutorial Request");

            return;
        }

        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to create Team for Tutorial Request");

            return;
        }

        // Schedule creation of Team for tutorial
        CreateTeamTeamOperation operation = new(
            MicrosoftTeamsHelper.FormatTeamName(tutorialRequest.Plan.Name),
            $"8912;TUT;Support;{_dateTime.CurrentYearAsString};{tutorialRequest.Grade.AsName()};{tutorialRequest.Plan.Name};");

        _teamOperationRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
