namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Application.Domains.Offerings.Queries.GetOfferingLocationsAsMapLayers;
using Application.Domains.Offerings.Queries.GetOfferingSummary;
using Constellation.Application.Domains.Offerings.Models;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class MapModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public MapModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<MapModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;
    [ViewData] public string PageTitle => "Map Data";

    [BindProperty(SupportsGet = true)]
    public OfferingId Id { get; set; } = OfferingId.Empty;

    public List<MapLayer> Layers { get; set; } = new();
    public string OfferingName { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve map of locations by user {User}", _currentUserService.UserName);

        Result<OfferingSummaryResponse> offeringResult = await _mediator.Send(new GetOfferingSummaryQuery(Id));

        if (offeringResult.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), offeringResult.Error, true)
                .Warning("Failed to retrieve map of locations by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                offeringResult.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

            return;
        }

        OfferingName = offeringResult.Value.Name;

        Result<List<MapLayer>> layers = await _mediator.Send(new GetOfferingLocationsAsMapLayersQuery(Id));

        if (layers.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), layers.Error, true)
                .Warning("Failed to retrieve map of locations by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                layers.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

            return;
        }
        
        Layers = layers.Value;
    }
}
