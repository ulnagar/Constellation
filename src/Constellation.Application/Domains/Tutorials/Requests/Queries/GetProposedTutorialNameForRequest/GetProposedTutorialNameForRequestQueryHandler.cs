namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetProposedTutorialNameForRequest;

using Abstractions.Messaging;
using Core.Extensions;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Models.Tutorials.ValueObjects;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetProposedTutorialNameForRequestQueryHandler
: IQueryHandler<GetProposedTutorialNameForRequestQuery, TutorialName>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public GetProposedTutorialNameForRequestQueryHandler(
        ITutorialRepository tutorialRepository,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _logger = logger
            .ForContext<GetProposedTutorialNameForRequestQuery>();
    }

    public async Task<Result<TutorialName>> Handle(GetProposedTutorialNameForRequestQuery request, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(request.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(GetProposedTutorialNameForRequestQuery), request, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(request.RequestId), true)
                .Warning("Failed to retrieve Tutorial Request with Id {id}", request.RequestId);

            return Result.Failure<TutorialName>(TutorialRequestErrors.NotFound(request.RequestId));
        }

        TutorialName proposedName = TutorialName.FromValue($"{tutorialRequest.Grade.AsNumber()}{tutorialRequest.Student.PreferredName[0]}{tutorialRequest.Student.LastName[0..1]}T");

        int sequence = await _tutorialRepository.GetNextTutorialNameSequence(proposedName, cancellationToken);

        TutorialName confirmedName = TutorialName.FromValue($"{proposedName.Value}{sequence + 1}");

        return confirmedName;
    }
}