namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetAllNominationPeriods;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Awards;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllNominationPeriodsQueryHandler
    : IQueryHandler<GetAllNominationPeriodsQuery, List<NominationPeriodResponse>>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly ILogger _logger;

    public GetAllNominationPeriodsQueryHandler(
        IAwardNominationRepository nominationRepository,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _logger = logger.ForContext<GetAllNominationPeriodsQuery>();
    }

    public async Task<Result<List<NominationPeriodResponse>>> Handle(GetAllNominationPeriodsQuery request, CancellationToken cancellationToken)
    {
        List<NominationPeriodResponse> responses = new();

        List<NominationPeriod> periods = await _nominationRepository.GetAll(cancellationToken);

        foreach (NominationPeriod period in periods)
        {
            List<Grade> grades = period
                .IncludedGrades
                .Select(entry => entry.Grade)
                .ToList();

            responses.Add(new(
                period.Id,
                period.Name,
                period.LockoutDate,
                grades));
        }

        return responses;
    }
}
