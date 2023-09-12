namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingLocationsAsMapLayers;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public List<MapLayer> Layers { get; set; } = new();
    public string OfferingName { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<OfferingSummaryResponse> offeringResult = await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

        if (offeringResult.IsFailure)
        {
            Error = new()
            {
                Error = offeringResult.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Index", values: new { area = "Subject" })
            };

            return;
        }

        OfferingName = offeringResult.Value.Name;

        Result<List<MapLayer>> layers = await _mediator.Send(new GetOfferingLocationsAsMapLayersQuery(offeringId));
    }
}
