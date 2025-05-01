namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffIntegratedTimetableData;

using Abstractions.Messaging;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffIntegratedTimetableDataQueryHandler
: IQueryHandler<GetStaffIntegratedTimetableDataQuery, List<StaffIntegratedTimetableResponse>>
{
    private readonly IPeriodRepository _periodRepository;
    private readonly IOfferingRepository _offeringRepository;

    public GetStaffIntegratedTimetableDataQueryHandler(
        IPeriodRepository periodRepository,
        IOfferingRepository offeringRepository)
    {
        _periodRepository = periodRepository;
        _offeringRepository = offeringRepository;
    }

    public async Task<Result<List<StaffIntegratedTimetableResponse>>> Handle(GetStaffIntegratedTimetableDataQuery request, CancellationToken cancellationToken)
    {
        List<StaffIntegratedTimetableResponse> response = [];

        List<Period> periods = await _periodRepository.GetAll(cancellationToken);

        List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(request.StaffId, cancellationToken);

        foreach (Period period in periods.Where(period => !period.IsDeleted))
        {
            List<Offering> matchingOfferings = offerings
                .Where(offering => offering.Sessions
                    .Any(session => session.PeriodId == period.Id))
                .ToList();

            if (matchingOfferings.Count == 0)
            {
                response.Add(new(
                    period.Id,
                    period.Timetable,
                    period.Week,
                    period.DayNumber,
                    period.Day,
                    $"{(period.Timetable.Prefix == '\0' ? string.Empty : period.Timetable.Prefix)}{period.PeriodCode}",
                    period.Name,
                    period.Type,
                    period.StartTime,
                    period.EndTime,
                    period.Duration,
                    OfferingName.None));

                continue;
            }

            response.Add(new(
                period.Id,
                period.Timetable,
                period.Week,
                period.DayNumber,
                period.Day,
                $"{(period.Timetable.Prefix == '\0' ? string.Empty : period.Timetable.Prefix)}{period.PeriodCode}",
                period.Name,
                period.Type,
                period.StartTime,
                period.EndTime,
                period.Duration,
                matchingOfferings.First().Name));
        }

        return response;
    }
}
