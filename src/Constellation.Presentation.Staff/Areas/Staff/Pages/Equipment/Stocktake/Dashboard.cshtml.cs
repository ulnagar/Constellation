namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEvent;
using Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForStaffMember;
using Application.Models.Auth;
using Constellation.Application.Domains.AssetManagement.Stocktake.Models;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DashboardModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DashboardModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DashboardModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_Dashboard;
    [ViewData] public string PageTitle => "Stocktake Dashboard";


    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public List<StocktakeSightingResponse> Sightings { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty)
        {
            return RedirectToPage("/Dashboard", new { area = "Staff" });
        }

        _logger
            .Information("Requested to view Stocktake Dashboard by user {User}", _currentUserService.UserName);
        
        string claimStaffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(claimStaffId))
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Auth.UserNotFound,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to view Stocktake Dashboard by user {User}", _currentUserService.UserName);

            return Page();
        }

        Guid guidStaffId = Guid.Parse(claimStaffId);
        StaffId staffId = StaffId.FromValue(guidStaffId);

        Result<StocktakeEventResponse> eventRequest = await _mediator.Send(new GetStocktakeEventQuery(Id));

        if (eventRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                eventRequest.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), eventRequest.Error, true)
                .Warning("Failed to view Stocktake Dashboard by user {User}", _currentUserService.UserName);

            return Page();
        }

        Result<List<StocktakeSightingResponse>> request = await _mediator.Send(new GetStocktakeSightingsForStaffMemberQuery(staffId, Id));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to view Stocktake Dashboard by user {User}", _currentUserService.UserName);

            return Page();
        }

        Sightings = request.Value;
        Name = eventRequest.Value.Name;
        StartDate = DateOnly.FromDateTime(eventRequest.Value.StartDate);
        EndDate = DateOnly.FromDateTime(eventRequest.Value.EndDate);

        return Page();
    }
}