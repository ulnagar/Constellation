namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Application.Common.PresentationModels;
using Constellation.Application.Domains.Faculties.Queries.GetFacultiesSummary;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;
    [ViewData] public string PageTitle => "Faculties List";

    public List<FacultySummaryResponse> Faculties { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Faculties by user {User}", _currentUserService.UserName);

        Result<List<FacultySummaryResponse>> faculties = await _mediator.Send(new GetFacultiesSummaryQuery());

        if (faculties.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(faculties.Error);

            _logger
                .ForContext(nameof(Error), faculties.Error, true)
                .Warning("Failed to retrieve list of Faculties by user {User}", _currentUserService.UserName);

            return;
        }

        Faculties = faculties.Value;
    }
}
