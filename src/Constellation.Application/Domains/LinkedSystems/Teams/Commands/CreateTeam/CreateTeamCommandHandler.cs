namespace Constellation.Application.Domains.LinkedSystems.Teams.Commands.CreateTeam;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.LinkedSystems;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Repositories;
using Constellation.Core.Models.Tutorials.ValueObjects;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

public sealed class CreateTeamCommandHandler 
    : ICommandHandler<CreateTeamCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITeamRepository _teamRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IDateTimeProvider _dateTime;

    public CreateTeamCommandHandler(
        ITeamRepository teamRepository,
        ITutorialRepository tutorialRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _teamRepository = teamRepository;
        _tutorialRepository = tutorialRepository;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<CreateTeamCommand>();
    }

    public async Task<Result> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        Team? checkTeam = await _teamRepository.GetById(request.Id, cancellationToken);

        if (checkTeam != null)
        {
            _logger
                .ForContext(nameof(CreateTeamCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.LinkedSystems.Teams.AlreadyExists(request.Id), true)
                .Warning("Failed to register Team in the database");

            return Result.Failure(DomainErrors.LinkedSystems.Teams.AlreadyExists(request.Id));
        }

        Team team = Team.Create(
            request.Id,
            request.Name,
            request.Description,
            request.ChannelId
        );

        _teamRepository.Insert(team);

        if (request.Description.Contains(";TUT;", StringComparison.InvariantCultureIgnoreCase)) 
        { 
            string[] tokens = request.Description.Split(';');

            Tutorial tutorial = null;

            foreach (string token in tokens)
            {
                if (tutorial is not null)
                    continue;

                TutorialName tutorialName = TutorialName.FromValue(token.Trim());

                tutorial = await _tutorialRepository.GetByNameAndYear(_dateTime.CurrentYear, tutorialName, cancellationToken);
            }

            if (tutorial is not null && tutorial.Teams.All(resource => resource.TeamId != team.Id))
            {
                tutorial.AddTeam(team.Id, team.Name, team.Link);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
