namespace Constellation.Application.Domains.Compliance.Wellbeing.Queries.GetWellbeingReportFromSentral;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Services;
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
        (Stream BasicReport, Stream DetailReport) files = await _sentralGateway.GetNAwardReport(cancellationToken);

        if (files.BasicReport.Length > 0 && files.DetailReport.Length > 0)
        {
            List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(_dateTime.CurrentYearAsString);

            List<SentralIncidentDetails> data = await _excelService.ConvertSentralIncidentReport(files.BasicReport, files.DetailReport, excludedDates, cancellationToken);

            return data;
        }

        return Result.Failure<List<SentralIncidentDetails>>(new("Gateway.Sentral.NoValue", "Could not access the Sentral Page required"));
    }
}
