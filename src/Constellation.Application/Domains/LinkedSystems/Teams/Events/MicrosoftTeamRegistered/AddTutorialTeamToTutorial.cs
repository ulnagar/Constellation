namespace Constellation.Application.Domains.LinkedSystems.Teams.Events.MicrosoftTeamRegistered;

using Abstractions.Messaging;
using Constellation.Core.Models.LinkedSystems;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.DomainEvents;
using Core.Models;
using Core.Models.LinkedSystems.Errors;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Models.Tutorials.ValueObjects;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public sealed class AddTutorialTeamToTutorial : IDomainEventHandler<MicrosoftTeamRegisteredDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTutorialTeamToTutorial(
        ITutorialRepository tutorialRepository,
        ITeamRepository teamRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _teamRepository = teamRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<MicrosoftTeamRegisteredDomainEvent>();
    }

    public async Task Handle(MicrosoftTeamRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        Team team = await _teamRepository.GetById(notification.TeamId, cancellationToken);

        if (team is null)
        {
            _logger
                .ForContext(nameof(MicrosoftTeamRegisteredDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.NotFound(notification.TeamId), true)
                .Warning("Failed to link Team with Tutorial");

            return;
        }

        string[] descriptionParts = team.Description.Split(';');

        if (!descriptionParts.Contains("TUT"))
        {
            _logger
                .ForContext(nameof(MicrosoftTeamRegisteredDomainEvent), notification, true)
                .Information("Team is not for a Tutorial");

            return;
        }

        string pattern = @"\d{2}T[a-zA-Z]{2}X\d";
        string tutName = string.Empty;

        foreach (string part in descriptionParts)
        {
            Match match = Regex.Match(part, pattern);

            if (match.Success)
                tutName = match.Value;
        }

        if (string.IsNullOrWhiteSpace(tutName))
        {
            _logger
                .ForContext(nameof(MicrosoftTeamRegisteredDomainEvent), notification, true)
                .ForContext(nameof(Team), team, true)
                .ForContext(nameof(Error), TeamErrors.NoTutorialName, true)
                .Warning("Failed to link Team with Tutorial");

            return;
        }

        TutorialName tutorialName = TutorialName.FromValue(tutName);

        Tutorial tutorial = await _tutorialRepository.GetByNameAndYear(_dateTime.CurrentYear, tutorialName, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(MicrosoftTeamRegisteredDomainEvent), notification, true)
                .ForContext(nameof(Team), team, true)
                .ForContext(nameof(Error), TutorialErrors.NotFoundByName(tutorialName), true)
                .Warning("Failed to link Team with Tutorial");

            return;
        }

        if (tutorial.Teams.Any(resource => resource.TeamId == team.Id))
            return;

        Result addTeamResult = tutorial.AddTeam(team.Id, team.Name, team.Link);

        if (addTeamResult.IsFailure)
        {
            _logger
                .ForContext(nameof(MicrosoftTeamRegisteredDomainEvent), notification, true)
                .ForContext(nameof(Team), team, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), addTeamResult.Error, true)
                .Warning("Failed to link Team with Tutorial");

            return;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}