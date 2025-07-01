namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Stocktake.Commands.CancelSighting;
using Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEvent;
using Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForStaffMember;
using Application.Models.Auth;
using Constellation.Application.Domains.AssetManagement.Stocktake.Models;
using Constellation.Application.Domains.Offerings.Commands.RemoveSession;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Stocktake.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.PartialViews.DeleteStocktakeSightingConfirmationModal;

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
    public StocktakeEventId Id { get; set; }

    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public List<StocktakeSightingResponse> Sightings { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (Id == StocktakeEventId.Empty)
        {
            return RedirectToPage("/Dashboard", new { area = "Staff" });
        }

        _logger
            .Information("Requested to view Stocktake Dashboard by user {User}", _currentUserService.UserName);
        
        string claimStaffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(claimStaffId))
        {
            ModalContent = ErrorDisplay.Create(
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
            ModalContent = ErrorDisplay.Create(
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
            ModalContent = ErrorDisplay.Create(
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

    public IActionResult OnPostAjaxDeleteSighting(
        StocktakeEventId eventId,
        StocktakeSightingId sightingId)
    {
        DeleteStocktakeSightingConfirmationModalViewModel viewModel = new(
            eventId,
            sightingId);

        return Partial("DeleteStocktakeSightingConfirmationModal", viewModel);
    }

    public async Task<IActionResult> OnPostDeleteSighting(DeleteStocktakeSightingConfirmationModalViewModel viewModel)
    {
        CancelSightingCommand command = new(
            viewModel.EventId,
            viewModel.SightingId,
            viewModel.Comment);

        _logger
            .ForContext(nameof(CancelSightingCommand), command, true)
            .Information("Requested to remove sighting by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove sighting by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff", Id }));

            return Page();
        }

        return RedirectToPage();
    }
}