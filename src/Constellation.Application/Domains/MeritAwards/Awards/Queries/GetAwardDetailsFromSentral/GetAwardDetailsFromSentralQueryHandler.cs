namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardDetailsFromSentral;

using Abstractions.Messaging;
using Core.Shared;
using Extensions;
using HtmlAgilityPack;
using Interfaces.Gateways;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAwardDetailsFromSentralQueryHandler
: IQueryHandler<GetAwardDetailsFromSentralQuery, List<AwardDetailResponse>>
{
    private readonly ISentralGateway _gateway;
    private readonly ILogger _logger;
    private readonly Regex _parser = new(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
    
    public GetAwardDetailsFromSentralQueryHandler(
        ISentralGateway gateway,
        ILogger logger)
    {
        _gateway = gateway;
        _logger = logger.ForContext<GetAwardDetailsFromSentralQuery>();
    }

    public async Task<Result<List<AwardDetailResponse>>> Handle(GetAwardDetailsFromSentralQuery request, CancellationToken cancellationToken)
    {
        List<AwardDetailResponse> response = new();

        HtmlDocument document = await _gateway.GetAwardsReport(cancellationToken);

        if (document is null)
        {
            _logger.Warning("Failed to retrieve Award Report from Sentral");
            
            return Result.Failure<List<AwardDetailResponse>>(new(
                "ExternalGateway.Sentral",
                "Failed to retrieve the Award Report from Sentral"));
        }

        List<string> list = document
            .DocumentNode
            .InnerHtml
            .Split('\u000A')
            .ToList();

        // Remove first and last entry
        list.RemoveAt(0);
        list.RemoveAt(list.Count - 1);

        for (int i = 0; i < list.Count; i++)
        {
            string entry = list[i];
            string[] split = _parser.Split(entry);

            // Index 0 = Award Category
            // Index 1 = Award Type
            // Index 2 = Awarded Date
            // Index 3 = Award Created DateTime
            // Index 4 = Award Source
            // Index 5 = Student Id (Sentral)
            // Index 6 = Student Id (StudentId)
            // Index 7 = First Name
            // Index 8 = Last Name

            bool success = DateTime.TryParse(split[2].FormatField(), out DateTime awardedDate);
            if (!success)
                continue;

            success = DateTime.TryParse(split[3].FormatField(), out DateTime awardCreated);
            if (!success)
                continue;

            awardCreated = awardCreated.AddSeconds(-awardCreated.Second);

            response.Add(new(
                split[0].FormatField(),
                split[1].FormatField(),
                awardedDate,
                awardCreated,
                split[4].FormatField(),
                split[5].FormatField(),
                split[6].FormatField(),
                split[7].FormatField(),
                split[8].FormatField()));
        }

        return response;
    }
}
