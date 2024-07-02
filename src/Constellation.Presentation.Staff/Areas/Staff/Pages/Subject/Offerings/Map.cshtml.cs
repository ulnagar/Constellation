namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingLocationsAsMapLayers;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class MapModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public MapModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public List<MapLayer> Layers { get; set; } = new();
    public string OfferingName { get; set; }

    public async Task OnGet()
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<OfferingSummaryResponse> offeringResult = await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

        if (offeringResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                offeringResult.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

            return;
        }

        OfferingName = offeringResult.Value.Name;

        Result<List<MapLayer>> layers = await _mediator.Send(new GetOfferingLocationsAsMapLayersQuery(offeringId));

        if (layers.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                layers.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

            return;
        }
        
        Layers = layers.Value;
    }
}
