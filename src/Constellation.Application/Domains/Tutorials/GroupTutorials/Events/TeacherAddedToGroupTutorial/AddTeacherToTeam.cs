namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Events.TeacherAddedToGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.LinkedSystems.Errors;
using Constellation.Core.Models.Operations.Enums;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Models.LinkedSystems;
using Core.Models.Operations;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.ValueObjects;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeacherToTeam
    : IDomainEventHandler<TeacherAddedToGroupTutorialDomainEvent>
{
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTeacherToTeam(
        ITeamOperationRepository operationsRepository,
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _operationsRepository = operationsRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<TeacherAddedToGroupTutorialDomainEvent>();
    }

    public async Task Handle(TeacherAddedToGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
    {
        GroupTutorial? tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.GroupTutorials.GroupTutorial.NotFound(notification.TutorialId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        TutorialTeacher? teacher = tutorial.Teachers.FirstOrDefault(teacher => teacher.Id == notification.TutorialTeacherId);

        if (teacher is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.GroupTutorials.TutorialTeacher.NotFound, true)
                .Error("Failed to complete the event handler");

            return;
        }

        StaffMember? staffMember = await _staffRepository.GetById(teacher.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(teacher.StaffId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        if (staffMember.EmailAddress == EmailAddress.None)
            return;

        string teamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name);

        List<Team> teams = await _teamRepository.GetByName(teamName, cancellationToken);

        if (teams.Count == 0)
        {
            _logger
                .ForContext(nameof(TeacherAddedToGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.NotFoundByName(teamName))
                .Error("Failed to complete the event handler");

            return;
        }

        if (teams.Count > 1)
        {
            _logger
                .ForContext(nameof(TeacherAddedToGroupTutorialDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.TooManyResults(teamName))
                .Error("Failed to complete the event handler");

            return;
        }

        ModifyTeamMembershipTeamOperation operation = new(
            teams.First().Id,
            staffMember.EmailAddress,
            TeamAction.AddOwner);
        
        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
