namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.Wellbeing;

using Application.Common.PresentationModels;
using Application.Domains.Compliance.Wellbeing.Queries.ExportWellbeingReport;
using Application.Domains.Compliance.Wellbeing.Queries.GetWellbeingReportFromSentral;
using Application.DTOs;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_Wellbeing;
    [ViewData] public string PageTitle => "N-Award Incident Listing";

    public List<SentralIncidentDetails> Data { get; set; } = new();

    public async Task OnGet() { }

    public async Task OnGetUpdate()
    {
        _logger.Information("Requested to retrieve N-Award Incidents from Sentral by user {User}", _currentUserService.UserName);

        Result<List<SentralIncidentDetails>> request = await _mediator.Send(new GetWellbeingReportFromSentralQuery());

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve N-Award Incidents from Sentral by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        Data = request.Value;
    }

    public async Task<IActionResult> OnGetExport()
    {
        _logger.Information("Requested to export N-Award Incidents from Sentral by user {User}", _currentUserService.UserName);

        Result<List<SentralIncidentDetails>> dataRequest = await _mediator.Send(new GetWellbeingReportFromSentralQuery());

        if (dataRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), dataRequest.Error, true)
                .Warning("Failed to export N-Award Incidents from Sentral by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(dataRequest.Error);

            return Page();
        }

        Result<FileDto> fileRequest = await _mediator.Send(new ExportWellbeingReportCommand(dataRequest.Value));

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to export N-Award Incidents from Sentral by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(fileRequest.Error);

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}