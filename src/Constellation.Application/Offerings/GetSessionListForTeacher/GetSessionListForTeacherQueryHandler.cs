namespace Constellation.Application.Offerings.GetSessionListForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSessionListForTeacherQueryHandler
    : IQueryHandler<GetSessionListForTeacherQuery, List<TeacherSessionResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetSessionListForTeacherQueryHandler(
        IOfferingRepository offeringRepository,
        ITimetablePeriodRepository periodRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _logger = logger;
    }

    public async Task<Result<List<TeacherSessionResponse>>> Handle(GetSessionListForTeacherQuery request, CancellationToken cancellationToken)
    {
        List<TeacherSessionResponse> response = new();

        List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(request.StaffId, cancellationToken);

        foreach (Session session in offerings.SelectMany(offering => offering.Sessions).ToList())
        {
            TimetablePeriod period = await _periodRepository.GetById(session.PeriodId, cancellationToken);

            if (period is null)
                continue;

            response.Add(new(
                session.Offering.Id,
                session.Offering.Name,
                session.Id,
                session.PeriodId,
                period.ToString(),
                period.Duration));
        }

        return response;
    }
}
