namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestById;

using Abstractions.Messaging;
using Constellation.Core.Models.Timetables;
using Constellation.Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialRequestByIdQueryHandler
:IQueryHandler<GetTutorialRequestByIdQuery, TutorialRequestDetailsResponse>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetTutorialRequestByIdQueryHandler(
        ITutorialRepository tutorialRepository,
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _periodRepository = periodRepository;
        _logger = logger;
    }

    public async Task<Result<TutorialRequestDetailsResponse>> Handle(GetTutorialRequestByIdQuery request, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(request.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(GetTutorialRequestByIdQuery), request, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(request.RequestId), true)
                .Warning("Failed to retrieve Tutorial Request");

            return Result.Failure<TutorialRequestDetailsResponse>(TutorialRequestErrors.NotFound(request.RequestId));
        }

        List<Period> periods = await _periodRepository.GetListFromIds(tutorialRequest.PeriodIds.ToList(), cancellationToken);

        return new TutorialRequestDetailsResponse(
            tutorialRequest.Id,
            tutorialRequest.Student,
            tutorialRequest.Grade,
            tutorialRequest.School,
            tutorialRequest.Type,
            tutorialRequest.Subject,
            periods,
            tutorialRequest.Justification,
            tutorialRequest.CreatedAt,
            tutorialRequest.Status,
            tutorialRequest.ReviewedBy,
            tutorialRequest.ReviewedAt,
            tutorialRequest.Notes);
    }
}
