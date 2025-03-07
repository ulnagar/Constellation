#nullable enable
namespace Constellation.Presentation.Staff.Areas.Staff.Pages;

using Application.Affirmations;
using Application.Stocktake.GetCurrentStocktakeEvents;
using Application.Training.GetCountOfExpiringCertificatesForStaffMember;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Application.StaffMembers.GetStaffByEmail;
using Constellation.Application.StaffMembers.Models;
using Constellation.Application.Stocktake.Models;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Offerings.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;
using Serilog;

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
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Staff_Dashboard;
    [ViewData] public string PageTitle => "Staff Dashboard";

    public string UserName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public List<RecentChange> Changes { get; set; } = new();

    public IReadOnlyList<StocktakeEventResponse> ActiveStocktakeEvents { get; set; } =
        new List<StocktakeEventResponse>();
    
    public Dictionary<string, OfferingId> Classes { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to load Staff Dashboard for user {User}", _currentUserService.UserName);

        string? username = User.Identity?.Name;
        
        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username), cancellationToken);

        Result<string> messageRequest = await _mediator.Send(new GetAffirmationQuery(teacherRequest.Value?.StaffId), cancellationToken);

        if (messageRequest.IsSuccess)
            Message = messageRequest.Value;

        Result<List<StocktakeEventResponse>>? stocktakeEvents = await _mediator.Send(new GetCurrentStocktakeEventsQuery(), cancellationToken);
        ActiveStocktakeEvents = stocktakeEvents.IsSuccess ? stocktakeEvents.Value : new List<StocktakeEventResponse>();

        var filepath = _environment.ContentRootPath;

        using (StreamReader r = new StreamReader(Path.Combine(filepath, "RecentUpdates.json")))
        {
            string json = await r.ReadToEndAsync(cancellationToken);
            var updates = JsonConvert.DeserializeObject<List<RecentChange>>(json);
            Changes = updates ?? new();
        }

        return Page();
    }

    public class RecentChange
    {
        public DateTime Datestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
