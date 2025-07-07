namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Application.Domains.Offerings.Queries.GetOfferingSummary;
using Application.Domains.Timetables.Periods.Queries.GetPeriodsForVisualSelection;
using Constellation.Application.Domains.Offerings.Commands.AddMultipleSessionsToOffering;
using Constellation.Application.Domains.Offerings.Models;
using Constellation.Application.Domains.Offerings.Queries.GetSessionListForOffering;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Timetables.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class AddSessionsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddSessionsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddSessionsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;
    [ViewData] public string PageTitle => "Bulk Add Sessions";

    [BindProperty(SupportsGet = true)]
    public OfferingId Id { get; set; }

    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    [BindProperty] 
    public List<PeriodId> Periods { get; set; } = new();

    public List<PeriodVisualSelectResponse> ValidPeriods { get; set; } = new();
    public List<SessionListResponse> ExistingSessions { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        AddMultipleSessionsToOfferingCommand command = new(Id, Periods);

        _logger
            .ForContext(nameof(AddMultipleSessionsToOfferingCommand), command, true)
            .Information("Requested to add bulk Sessions to Offering by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add bulk Sessions to Offering by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve defaults for Bulk Add Sessions to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<List<PeriodVisualSelectResponse>> periodRequest = await _mediator.Send(new GetPeriodsForVisualSelectionQuery());

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to retrieve defaults for Bulk Add Sessions to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        ValidPeriods = periodRequest.Value;

        // Get current periods linked to Offering
        Result<List<SessionListResponse>> sessionRequest = await _mediator.Send(new GetSessionListForOfferingQuery(Id));

        if (sessionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), sessionRequest.Error, true)
                .Warning("Failed to retrieve defaults for Bulk Add Sessions to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                sessionRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        ExistingSessions = sessionRequest.Value;

        // Get CourseName and OfferingName
        Result<OfferingSummaryResponse> offeringRequest = await _mediator.Send(new GetOfferingSummaryQuery(Id));

        if (offeringRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), offeringRequest.Error, true)
                .Warning("Failed to retrieve defaults for Bulk Add Sessions to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                offeringRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        CourseName = offeringRequest.Value.CourseName;
        OfferingName = offeringRequest.Value.Name;
    }
}