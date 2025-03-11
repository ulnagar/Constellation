namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Common.PresentationModels;
using Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Application.Models.Auth;
using Application.Schools.GetSchoolsSummaryList;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;
    [ViewData] public string PageTitle => "Schools List";

    [BindProperty(SupportsGet = true)]
    public SchoolFilter Filter { get; set; } = SchoolFilter.Active;

    public List<SchoolSummaryResponse> Schools { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Schools by user {User}", _currentUserService.UserName);

        Result<List<SchoolSummaryResponse>> result = await _mediator.Send(new GetSchoolsSummaryListQuery(Filter));

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retrieve list of Schools by user {User}", _currentUserService.UserName);

            return;
        }

        Schools = result.Value;
    }
}