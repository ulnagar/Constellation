namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Stocktake;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Stocktake.Queries.GetCurrentStocktakeEvents;
using Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForSchool;
using Application.Models.Auth;
using Constellation.Application.Domains.AssetManagement.Stocktake.Commands.CancelSighting;
using Constellation.Application.Domains.AssetManagement.Stocktake.Models;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Stocktake.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Schools.Pages.Shared.PartialViews.RemoveSightingConfirmation;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Stocktake;

    public SelectList StocktakeEvents { get; set; }

    public StocktakeEventResponse Stocktake { get; set; }

    [BindProperty(SupportsGet = true)]
    public StocktakeEventId EventId { get; set; } = StocktakeEventId.Empty;

    public List<StocktakeSightingResponse> Sightings { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve stocktake event data by user {user}", _currentUserService.UserName);

        Result<List<StocktakeEventResponse>> eventsRequest = await _mediator.Send(new GetCurrentStocktakeEventsQuery());

        if (!eventsRequest.IsSuccess)
        {
            ModalContent = ErrorDisplay.Create(eventsRequest.Error);

            return;
        }

        if (eventsRequest.Value.Count == 0)
        {
            ModalContent = ErrorDisplay.Create(
                new("No Stocktake", "Not current Stocktake Event found"),
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Schools" }));

            return;
        }

        if (EventId.Equals(StocktakeEventId.Empty))
        {
            EventId = eventsRequest.Value.First().Id;
        }

        StocktakeEvents = new SelectList(eventsRequest.Value,
            nameof(StocktakeEventResponse.Id),
            nameof(StocktakeEventResponse.Name),
            EventId);

        Stocktake = eventsRequest.Value.First(entry => entry.Id == EventId);

        _logger.Information("Requested to retrieve stocktake sightings by user {user} for event {eventName}", _currentUserService.UserName, Stocktake.Name);

        Result<List<StocktakeSightingResponse>> sightingsRequest = await _mediator.Send(new GetStocktakeSightingsForSchoolQuery(CurrentSchoolCode, EventId));

        if (sightingsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(sightingsRequest.Error);

            return;
        }

        Sightings = sightingsRequest.Value;
    }

    public async Task<IActionResult> OnPostAjaxRemoveSighting(StocktakeEventId eventId, StocktakeSightingId sightingId)
    {
        Result<List<StocktakeSightingResponse>> sightingsRequest = await _mediator.Send(new GetStocktakeSightingsForSchoolQuery(CurrentSchoolCode, EventId));
        StocktakeSightingResponse? sighting = sightingsRequest.Value.FirstOrDefault(entry => entry.Id == sightingId);

        if (sighting is null)
        {
            return Content(string.Empty);
        }

        RemoveSightingConfirmationViewModel viewModel = new()
        {
            EventId = eventId,
            SightingId = sightingId,
            SerialNumber = sighting.SerialNumber,
            AssetNumber = sighting.AssetNumber,
            Description = sighting.Description,
            SightedBy = sighting.SightedBy,
            SightedAt = DateOnly.FromDateTime(sighting.SightedAt),
            UserName = sighting.UserName
        };

        return Partial("RemoveSightingConfirmation", viewModel);
    }

    public async Task<IActionResult> OnPostRemoveSighting(RemoveSightingConfirmationViewModel viewModel)
    {
        CancelSightingCommand command = new(
            viewModel.EventId,
            viewModel.SightingId,
            viewModel.Comment);

        _logger.Information("Requested to remove stocktake sighting by user {user} with data {@command}", _currentUserService.UserName, command);

        Result result = await _mediator.Send(command);
        
        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", EventId }));

            return Page();
        }

        return RedirectToPage();
    }
}