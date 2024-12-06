namespace Constellation.Application.Periods.GetPeriodsForVisualSelection;

using Abstractions.Messaging;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetPeriodsForVisualSelectionQueryHandler
    : IQueryHandler<GetPeriodsForVisualSelectionQuery, List<PeriodVisualSelectResponse>>
{
    private readonly IPeriodRepository _periodRepository;

    public GetPeriodsForVisualSelectionQueryHandler(
        IPeriodRepository periodRepository)
    {
        _periodRepository = periodRepository;
    }

    public async Task<Result<List<PeriodVisualSelectResponse>>> Handle(GetPeriodsForVisualSelectionQuery request,
        CancellationToken cancellationToken)
    {
        List<PeriodVisualSelectResponse> response = new();

        List<Period> periods = await _periodRepository.GetCurrent(cancellationToken);

        foreach (Period period in periods)
        {
            response.Add(new(
                period.Id,
                period.Timetable,
                period.DayNumber,
                period.PeriodCode,
                period.StartTime,
                period.EndTime,
                period.Name,
                period.Type.Name,
                period.Duration));
        }

        return response;
    }
}