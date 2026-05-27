namespace Constellation.Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Shared;
using Interfaces.Gateways;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTermsAndWeeksForCurrentYearQueryHandler
: IQueryHandler<GetTermsAndWeeksForCurrentYearQuery, List<SchoolCalendarWeek>>
{
    private readonly ISentralGateway _gateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetTermsAndWeeksForCurrentYearQueryHandler(
        ISentralGateway gateway,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _gateway = gateway;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetTermsAndWeeksForCurrentYearQuery>();
    }

    public async Task<Result<List<SchoolCalendarWeek>>> Handle(GetTermsAndWeeksForCurrentYearQuery request, CancellationToken cancellationToken)
    {
        List<SchoolCalendarWeek> weekDescriptors = await _gateway.GetTermsAndWeeksFromApi(_dateTime.CurrentYearAsString, cancellationToken);

        return weekDescriptors;
    }
}
