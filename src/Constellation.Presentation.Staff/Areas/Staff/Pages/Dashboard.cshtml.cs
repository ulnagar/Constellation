#nullable enable
namespace Constellation.Presentation.Staff.Areas.Staff.Pages;

using Application.Domains.Affirmations.Queries;
using Application.Domains.AssetManagement.Stocktake.Queries.GetCurrentStocktakeEvents;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffByEmail;
using Constellation.Application.Domains.AssetManagement.Stocktake.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Reflection;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DashboardModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    public DashboardModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _environment = environment;
        _logger = logger
            .ForContext<DashboardModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Staff_Dashboard;
    [ViewData] public string PageTitle => "Staff Dashboard";

    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string VersionLabel { get; set; } = string.Empty;
    
    public IReadOnlyList<StocktakeEventResponse> ActiveStocktakeEvents { get; set; } =
        new List<StocktakeEventResponse>();
    
    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to load Staff Dashboard for user {User}", _currentUserService.UserName);

        string? username = User.Identity?.Name;
        
        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username), cancellationToken);

        if (teacherRequest.IsFailure)
            return Page();
        
        UserName = $"{teacherRequest.Value.FirstName} {teacherRequest.Value.LastName}";

        Result<string> messageRequest = await _mediator.Send(new GetAffirmationQuery(teacherRequest.Value?.StaffId), cancellationToken);

        if (messageRequest.IsSuccess)
            Message = messageRequest.Value;

        Result<List<StocktakeEventResponse>>? stocktakeEvents = await _mediator.Send(new GetCurrentStocktakeEventsQuery(), cancellationToken);
        ActiveStocktakeEvents = stocktakeEvents.IsSuccess ? stocktakeEvents.Value : new List<StocktakeEventResponse>();

        VersionLabel = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        return Page();
    }

}
