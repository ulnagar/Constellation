namespace Constellation.Application.Compliance.GetWellbeingReportFromSentral;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Core.Shared;
using Interfaces.Gateways;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetWellbeingReportFromSentralQueryHandler
: IQueryHandler<GetWellbeingReportFromSentralQuery, List<SentralIncidentDetails>>
{
    private readonly ISentralGateway _sentralGateway;
    private readonly IExcelService _excelService;

    public GetWellbeingReportFromSentralQueryHandler(
        ISentralGateway sentralGateway,
        IExcelService excelService)
    {
        _sentralGateway = sentralGateway;
        _excelService = excelService;
    }


    public async Task<Result<List<SentralIncidentDetails>>> Handle(GetWellbeingReportFromSentralQuery request, CancellationToken cancellationToken)
    {
        Stream file = await _sentralGateway.GetNAwardReport();

        if (file.Length > 0)
        {
            List<SentralIncidentDetails> data = await _excelService.ConvertSentralIncidentReport(file);

            return data;
        }

        return Result.Failure<List<SentralIncidentDetails>>(new("Gateway.Sentral.NoValue", "Could not access the Sentral Page required"));
    }
}
