namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards;

using Constellation.Application.Awards.GetAwardCountsByTypeByGrade;
using Constellation.Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DashboardModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DashboardModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DashboardModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Dashboard;
    [ViewData] public string PageTitle => "Awards Dashboard";

    public void OnGet() => _logger.Information("Requested to retrieve list of Awards by user {User}", _currentUserService.UserName);

    public async Task<IActionResult> OnPostGetData(CancellationToken cancellationToken = default)
    {
        Result<List<AwardCountByTypeByGradeResponse>> request = await _mediator.Send(new GetAwardCountsByTypeByGradeQuery(DateTime.Today.Year), cancellationToken);

        if (request.IsFailure)
        {
            return new JsonResult(null);
        }

        return new JsonResult(request.Value);
    }
}
