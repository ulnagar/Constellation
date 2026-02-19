namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardIncidentsFromSentral;

using Abstractions.Messaging;
using AppSettings.Models;
using Core.Errors;
using Core.Models.AppSettings.Enums;
using Core.Models.Awards;
using Core.Shared;
using HtmlAgilityPack;
using Interfaces.Gateways;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAwardIncidentsFromSentralQueryHandler
    : IQueryHandler<GetAwardIncidentsFromSentralQuery, List<AwardIncidentResponse>>
{
    private readonly ISentralGateway _gateway;
    private readonly IAppSettingsService _appSettings;
    private readonly ILogger _logger;

    public GetAwardIncidentsFromSentralQueryHandler(
        ISentralGateway gateway,
        IAppSettingsService appSettings,
        ILogger logger)
    {
        _gateway = gateway;
        _appSettings = appSettings;
        _logger = logger.ForContext<GetAwardIncidentsFromSentralQuery>();
    }

    public async Task<Result<List<AwardIncidentResponse>>> Handle(GetAwardIncidentsFromSentralQuery request, CancellationToken cancellationToken)
    {
        List<AwardIncidentResponse> response = new();
        
        HtmlDocument? page = await _gateway.GetAwardsListing(request.StudentId, request.Year, cancellationToken);

        if (page is null)
        {
            _logger
                .ForContext(nameof(GetAwardIncidentsFromSentralQuery), request, true)
                .ForContext(nameof(Error), ApplicationErrors.UnknownError, true)
                .Warning("Failed to retrieve Student Awards list from Sentral");

            return Result.Failure<List<AwardIncidentResponse>>(ApplicationErrors.UnknownError);
        }

        SentralConfiguration? awardsListPath = await _appSettings.Sentral(SentralPath.WellbeingStudentAwardsList, cancellationToken);

        if (awardsListPath is null)
        {
            _logger
                .ForContext(nameof(GetAwardIncidentsFromSentralQuery), request, true)
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(SentralConfiguration)), true)
                .Warning("Failed to retrieve Student Awards list from Sentral");

            return Result.Failure<List<AwardIncidentResponse>>(ApplicationErrors.InvalidConfiguration(nameof(SentralConfiguration)));
        }

        HtmlNode? awardsList = page.DocumentNode.SelectSingleNode(awardsListPath.Path);

        if (awardsList is null)
        {
            _logger
                .ForContext(nameof(GetAwardIncidentsFromSentralQuery), request, true)
                .ForContext(nameof(Error), ApplicationErrors.UnknownError, true)
                .Warning("Failed to retrieve Student Awards list from Sentral");

            return Result.Failure<List<AwardIncidentResponse>>(ApplicationErrors.UnknownError);
        }

        IEnumerable<HtmlNode> rows = awardsList.Descendants("tr");

        SentralConfiguration? incidentDatePath = await _appSettings.Sentral(SentralPath.IncidentCreatedDate, cancellationToken);

        if (incidentDatePath is null)
        {
            _logger
                .ForContext(nameof(GetAwardIncidentsFromSentralQuery), request, true)
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(SentralConfiguration)), true)
                .Warning("Failed to retrieve Student Awards list from Sentral");

            return Result.Failure<List<AwardIncidentResponse>>(ApplicationErrors.InvalidConfiguration(nameof(SentralConfiguration)));
        }

        foreach (HtmlNode row in rows)
        {
            int cellNumber = 0;

            DateTime createdOn = DateTime.MinValue;
            DateOnly issuedFor = DateOnly.MinValue;
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
                        bool issuedForParse = DateOnly.TryParse(cell.InnerText, out issuedFor);

                        if (!issuedForParse)
                        {
                            _logger
                                .ForContext("Sentral Date Field", cell.InnerText)
                                .Warning("Failed to extract date from Sentral Date Field");

                            continue;
                        }

                        break;
                    case 2:
                        // Incident link and type
                        string href = cell.ChildNodes.FindFirst("a").GetAttributeValue("href", "");

                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            // Date the award was created (i.e. when the award was entered into Sentral)
                            HtmlDocument? incidentPage = await _gateway.GetIncidentDetailsPage(href, cancellationToken);

                            if (incidentPage is not null)
                            {
                                HtmlNode? entry = incidentPage.DocumentNode.SelectSingleNode(incidentDatePath.Path);

                                if (entry is null)
                                {
                                    _logger
                                        .ForContext("Page Link", href)
                                        .Warning("Failed to extract Incident Created Date from page");

                                    continue;
                                }

                                //_logger
                                //    .ForContext("Text Incident Date", entry.InnerText.Trim())
                                //    .Information("Detected incident creation date");

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
