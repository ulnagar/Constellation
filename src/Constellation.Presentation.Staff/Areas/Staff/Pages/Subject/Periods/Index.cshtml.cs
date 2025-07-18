namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Periods;

using Application.Common.PresentationModels;
using Application.Domains.Timetables.Periods.Queries.GetPeriodsForVisualSelection;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Periods_Periods;
    [ViewData] public string PageTitle => "Timetable View";

    public List<PeriodVisualSelectResponse> Periods { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Periods by user {User}", _currentUserService.UserName);

        Result<List<PeriodVisualSelectResponse>> request = await _mediator.Send(new GetPeriodsForVisualSelectionQuery());

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of Periods by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        Periods = request.Value;
    }
}