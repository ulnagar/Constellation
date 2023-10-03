namespace Constellation.Application.Offerings.GetSessionListForOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSessionListForOfferingQueryHandler
    : IQueryHandler<GetSessionListForOfferingQuery, List<SessionListResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetSessionListForOfferingQueryHandler(
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetSessionListForOfferingQuery>();
    }

    public async Task<Result<List<SessionListResponse>>> Handle(GetSessionListForOfferingQuery request, CancellationToken cancellationToken)
    {
        List<SessionListResponse> response = new();

        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(GetSessionListForOfferingQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Could not retrieve list of Sessions for Offering");

            return Result.Failure<List<SessionListResponse>>(OfferingErrors.NotFound(request.OfferingId));
        }

        List<Session> currentSessions = offering
            .Sessions
            .Where(session => !session.IsDeleted)
            .ToList();

        foreach (Session session in currentSessions)
        {
            response.Add(new(
                offering.Id,
                session.Id,
                session.PeriodId));
        }

        return response;
    }
}
