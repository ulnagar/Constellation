namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardDetailsFromSentral;

using Abstractions.Messaging;
using Core.Shared;
using Import.Models;
using Interfaces.Gateways;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAwardDetailsFromSentralQueryHandler
: IQueryHandler<GetAwardDetailsFromSentralQuery, List<AwardDetailResponse>>
{
    private readonly ISentralGateway _gateway;
    private readonly SentralAwardReportCsvParser _parser;
    private readonly ILogger _logger;
    
    public GetAwardDetailsFromSentralQueryHandler(
        ISentralGateway gateway,
        SentralAwardReportCsvParser parser,
        ILogger logger)
    {
        _gateway = gateway;
        _parser = parser;
        _logger = logger.ForContext<GetAwardDetailsFromSentralQuery>();
    }

    public async Task<Result<List<AwardDetailResponse>>> Handle(GetAwardDetailsFromSentralQuery request, CancellationToken cancellationToken)
    {
        Stream stream = await _gateway.GetAwardsReport(cancellationToken);

        Result<List<StudentAwardRow>> rows = _parser.Parse(stream);

        if (rows.IsFailure)
        {
            _logger
                .ForContext(nameof(GetAwardDetailsFromSentralQuery), request, true)
                .ForContext(nameof(Error), rows.Error, true)
                .Warning("Failed to retrieve Awards Listing from Sentral");

            return Result.Failure<List<AwardDetailResponse>>(rows.Error);
        }

        return rows.Value.Select(entry => AwardDetailResponse.FromCsv(entry)).ToList();
    }
}
