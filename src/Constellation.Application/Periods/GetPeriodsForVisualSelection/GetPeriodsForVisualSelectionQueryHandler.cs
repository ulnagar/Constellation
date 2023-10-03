namespace Constellation.Application.Periods.GetPeriodsForVisualSelection;

using Abstractions.Messaging;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetPeriodsForVisualSelectionQueryHandler
    : IQueryHandler<GetPeriodsForVisualSelectionQuery, List<PeriodVisualSelectResponse>>
{
    private readonly ITimetablePeriodRepository _periodRepository;

    public GetPeriodsForVisualSelectionQueryHandler(
        ITimetablePeriodRepository periodRepository)
    {
        _periodRepository = periodRepository;
    }

    public async Task<Result<List<PeriodVisualSelectResponse>>> Handle(GetPeriodsForVisualSelectionQuery request,
        CancellationToken cancellationToken)
    {
        List<PeriodVisualSelectResponse> response = new();

        List<TimetablePeriod> periods = await _periodRepository.GetCurrent(cancellationToken);

        foreach (TimetablePeriod period in periods)
        {
            response.Add(new(
                period.Id,
                period.Timetable,
                period.Day,
                period.Period,
                period.StartTime,
                period.EndTime,
                period.Name,
                period.Type,
                period.Duration));
        }

        return response;
    }
}