namespace Constellation.Application.Compliance.GetWellbeingReportFromSentral;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Clock;
using Core.Shared;
using Interfaces.Gateways;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetWellbeingReportFromSentralQueryHandler
: IQueryHandler<GetWellbeingReportFromSentralQuery, List<SentralIncidentDetails>>
{
    private readonly ISentralGateway _sentralGateway;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;

    public GetWellbeingReportFromSentralQueryHandler(
        ISentralGateway sentralGateway,
        IExcelService excelService,
        IDateTimeProvider dateTime)
    {
        _sentralGateway = sentralGateway;
        _excelService = excelService;
        _dateTime = dateTime;
    }


    public async Task<Result<List<SentralIncidentDetails>>> Handle(GetWellbeingReportFromSentralQuery request, CancellationToken cancellationToken)
    {
        Stream file = await _sentralGateway.GetNAwardReport();

        if (file.Length > 0)
        {
            List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(_dateTime.CurrentYear.ToString());

            List<SentralIncidentDetails> data = await _excelService.ConvertSentralIncidentReport(file, excludedDates, cancellationToken);

            return data;
        }

        return Result.Failure<List<SentralIncidentDetails>>(new("Gateway.Sentral.NoValue", "Could not access the Sentral Page required"));
    }
}
