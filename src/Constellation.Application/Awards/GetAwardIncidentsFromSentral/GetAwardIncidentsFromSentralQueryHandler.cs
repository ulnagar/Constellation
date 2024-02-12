namespace Constellation.Application.Awards.GetAwardIncidentsFromSentral;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Configuration;
using Core.Models.Awards;
using Core.Shared;
using HtmlAgilityPack;
using Interfaces.Gateways;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAwardIncidentsFromSentralQueryHandler
    : IQueryHandler<GetAwardIncidentsFromSentralQuery, List<AwardIncidentResponse>>
{
    private readonly ISentralGateway _gateway;
    private readonly SentralGatewayConfiguration _settings;
    private readonly ILogger _logger;

    public GetAwardIncidentsFromSentralQueryHandler(
        ISentralGateway gateway,
        IOptions<SentralGatewayConfiguration> settings,
        ILogger logger)
    {
        _gateway = gateway;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<List<AwardIncidentResponse>>> Handle(GetAwardIncidentsFromSentralQuery request, CancellationToken cancellationToken)
    {
        List<AwardIncidentResponse> response = new();

        HtmlDocument page = await _gateway.GetAwardsListing(request.StudentId, request.Year, cancellationToken);

        HtmlNode awardsList = page.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "WellbeingStudentAwardsList").Value);

        if (awardsList is null) return response;
        
        IEnumerable<HtmlNode> rows = awardsList.Descendants("tr");

        foreach (HtmlNode row in rows)
        {
            int cellNumber = 0;

            DateTime createdOn = DateTime.MinValue;
            DateOnly issuedFor;
            string incidentId = string.Empty;
            string teacherName = string.Empty;
            string issueReason = string.Empty;
                
            foreach (HtmlNode cell in row.Descendants("td"))
            {
                cellNumber++;
                    
                switch (cellNumber)
                {
                    case 1:
                        // Date the award was for (i.e. events on this date are the reason for the award)
                        DateOnly.TryParse(cell.InnerText, out issuedFor);

                        break;
                    case 2:
                        // Incident link and type
                        string href = cell.ChildNodes.FindFirst("a").GetAttributeValue("href", "");

                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            // Date the award was created (i.e. when the award was entered into Sentral)
                            HtmlDocument incidentPage = await _gateway.GetIncidentDetailsPage(href, cancellationToken);

                            if (incidentPage is not null)
                            {
                                HtmlNode entry = incidentPage.DocumentNode.SelectSingleNode(_settings.XPaths.First(a => a.Key == "IncidentCreatedDate").Value);

                                string text = entry.InnerText.Trim();
                                string[] split = text.Split(' ');
                                    
                                if (split[1].Contains("on"))
                                {
                                    string dateTimeString = $"{split[2]} {split[4]}";
                                    bool success = DateTime.TryParse(dateTimeString, out createdOn);

                                    if (!success)
                                    {
                                        _logger
                                            .ForContext("String Date", dateTimeString)
                                            .Warning("Failed to extract date from Sentral Incident timestamp");
                                            
                                        continue;
                                    }
                                }
                                else if (split[1].Contains("at"))
                                {
                                    string dateTimeString = $"{issuedFor} {split[2]}";
                                    bool success = DateTime.TryParse(dateTimeString, out createdOn);

                                    if (!success)
                                    {
                                        _logger
                                            .ForContext("String Date", dateTimeString)
                                            .Warning("Failed to extract date from Sentral Incident timestamp");

                                        continue;
                                    }
                                }
                            }

                            incidentId = href.Split('=')[1].Split('&')[0];
                        }

                        break;
                    case 3:
                        // Incident Type
                        if (cell.InnerText.Trim() != StudentAward.Astra)
                            continue;

                        break;
                    case 4:
                        // issuing teacher name
                        string[] name = cell.InnerText.Split(',');

                        teacherName = $"{name[1].Trim()} {name[0].Trim()}";

                        break;
                    case 6:
                        // Issuing reason
                        issueReason = cell.InnerText.Trim();

                        break;
                }
            }

            response.Add(new(
                createdOn,
                issuedFor,
                incidentId,
                teacherName,
                issueReason));
        }

        return response;
    }
}
