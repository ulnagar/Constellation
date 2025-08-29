namespace Constellation.Application.Domains.Tutorials.Queries.GetSessionListForTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSessionListForTutorialQueryHandler
: IQueryHandler<GetSessionListForTutorialQuery, List<SessionListResponse>>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public GetSessionListForTutorialQueryHandler(
        ITutorialRepository tutorialRepository,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _logger = logger;
    }

    public async Task<Result<List<SessionListResponse>>> Handle(GetSessionListForTutorialQuery request, CancellationToken cancellationToken)
    {
        List<SessionListResponse> response = new();

        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(GetSessionListForTutorialQuery), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Could not retrieve list of Sessions for Tutorial");

            return Result.Failure<List<SessionListResponse>>(TutorialErrors.NotFound(request.TutorialId));
        }

        List<TutorialSession> currentSessions = tutorial
            .Sessions
            .Where(session => !session.IsDeleted)
            .ToList();

        foreach (TutorialSession session in currentSessions)
        {
            response.Add(new(
                tutorial.Id,
                session.Id,
                session.PeriodId,
                session.StaffId));
        }

        return response;
    }
}
