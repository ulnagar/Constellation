namespace Constellation.Infrastructure.Jobs;

using Application.Compliance.GetWellbeingReportFromSentral;
using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways;
using Application.Interfaces.Jobs;
using Application.Interfaces.Services;
using Application.WorkFlows.AddCaseDetailUpdateAction;
using Application.WorkFlows.CreateComplianceCase;
using Core.Abstractions.Clock;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using Microsoft.Extensions.Options;

internal sealed class SentralComplianceScanJob : ISentralComplianceScanJob
{
    private readonly ISender _mediator;
    private readonly ISentralGateway _sentralGateway;
    private readonly IExcelService _excelService;
    private readonly ICaseRepository _caseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _logger;

    public SentralComplianceScanJob(
        ISender mediator,
        ISentralGateway sentralGateway,
        IExcelService excelService,
        ICaseRepository caseRepository,
        IDateTimeProvider dateTime,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _mediator = mediator;
        _sentralGateway = sentralGateway;
        _excelService = excelService;
        _caseRepository = caseRepository;
        _dateTime = dateTime;
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        // Get all currently open N-Award and LoC from Sentral
        (Stream BasicFile, Stream DetailFile) files = await _sentralGateway.GetNAwardReport(cancellationToken);
        List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(_dateTime.CurrentYear.ToString());
        List<SentralIncidentDetails> incidents = await _excelService.ConvertSentralIncidentReport(files.BasicFile, files.DetailFile, excludedDates, cancellationToken);

        // Do not run on dates that are in the excluded list to prevent from processing the same values over and over (e.g. during school holidays)
        if (excludedDates.Contains(_dateTime.Today))
            return;
        
        // Get all entries that are currently 12 days overdue
        IEnumerable<SentralIncidentDetails> workingIncidents = incidents
            .Where(entry => entry.Severity == 12);
        
        foreach (SentralIncidentDetails incident in workingIncidents)
        {
            // Check for an existing WorkFlow for these items, and create one if not existing
            Case? existingCase = await _caseRepository.GetComplianceCaseForIncident(incident.IncidentId, cancellationToken);

            if (existingCase is not null && existingCase.Status.Equals(CaseStatus.Open))
            {
                // force a reminder email update

                AddCaseDetailUpdateActionCommand command = new(
                    existingCase.Id,
                    _configuration.WorkFlow.ComplianceReviewer,
                    $"Incident identified during scan on {_dateTime.Today.ToShortDateString()} as being {incident.Severity} days old.");

                await _mediator.Send(command, cancellationToken);

                continue;
            }

            await _mediator.Send(new CreateComplianceCaseCommand(incident), cancellationToken);
        }

        // Get all entries that are (age - 12 % 5 = 0) with open WorkFlows and force a reminder update

        IEnumerable<IGrouping<int, SentralIncidentDetails>> groupedAgeIncidents = incidents
            .Where(entry => entry.Severity > 12)
            .GroupBy(entry => entry.Severity);

        foreach (IGrouping<int, SentralIncidentDetails> group in groupedAgeIncidents.Where(group => group.Key > 12 && (group.Key - 12) % 5 == 0))
        {
            foreach (SentralIncidentDetails incident in group)
            {
                // Check for an existing WorkFlow for these items, and create one if not existing
                Case? existingCase = await _caseRepository.GetComplianceCaseForIncident(incident.IncidentId, cancellationToken);

                if (existingCase is not null && existingCase.Status.Equals(CaseStatus.Open))
                {
                    // force a reminder email update
                    AddCaseDetailUpdateActionCommand command = new(
                        existingCase.Id,
                        _configuration.WorkFlow.ComplianceReviewer,
                        $"Incident identified during scan on {_dateTime.Today.ToShortDateString()} as being {incident.Severity} days old.");

                    await _mediator.Send(command, cancellationToken);

                    continue;
                }

                await _mediator.Send(new CreateComplianceCaseCommand(incident), cancellationToken);
            }
        }
    }
}