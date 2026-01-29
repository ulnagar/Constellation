namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Events.StudentRemovedFromGroupTutorial;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.LinkedSystems.Errors;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Operations.Enums;
using Core.DomainEvents;
using Core.Models.LinkedSystems;
using Core.Models.Operations.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Helpers;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStudentFromTeam
    : IDomainEventHandler<StudentRemovedFromGroupTutorialDomainEvent>
{
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStudentFromTeam(
        ITeamOperationRepository operationsRepository,
        IGroupTutorialRepository groupTutorialRepository,
        IStudentRepository studentRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _operationsRepository = operationsRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _studentRepository = studentRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<StudentRemovedFromGroupTutorialDomainEvent>();
    }

    public async Task Handle(StudentRemovedFromGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
    {
        GroupTutorial? tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(StudentRemovedFromGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.GroupTutorials.GroupTutorial.NotFound(notification.TutorialId), true)
                .Error("Failed to complete the event handler");

            return;
        }
        
        TutorialEnrolment? enrolment = tutorial.Enrolments.FirstOrDefault(enrolment => enrolment.Id == notification.EnrolmentId);

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(StudentRemovedFromGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.GroupTutorials.TutorialEnrolment.NotFound, true)
                .Error("Failed to complete the event handler");

            return;
        }
        
        Student? student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentRemovedFromGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(enrolment.StudentId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        if (student.EmailAddress == EmailAddress.None)
            return;

        string teamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name);

        List<Team> teams = await _teamRepository.GetByName(teamName, cancellationToken);

        if (teams.Count == 0)
        {
            _logger
                .ForContext(nameof(StudentRemovedFromGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.NotFoundByName(teamName))
                .Error("Failed to complete the event handler");

            return;
        }

        if (teams.Count > 1)
        {
            _logger
                .ForContext(nameof(StudentRemovedFromGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.TooManyResults(teamName))
                .Error("Failed to complete the event handler");

            return;
        }

        ModifyTeamMembershipTeamOperation operation = new(
            teams.First().Id,
            student.EmailAddress,
            TeamAction.Remove);

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
