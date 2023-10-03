namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Periods.GetAllPeriods;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.Pages.Shared.Components.AddSessionToOffering;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AddSessionToOfferingViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public AddSessionToOfferingViewComponent(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid Id)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        Result<OfferingDetailsResponse> offering = await _mediator.Send(new GetOfferingDetailsQuery(offeringId));

        var viewModel = new AddSessionToOfferingSelection
        {
            OfferingId = offeringId,
            CourseName = offering.Value.CourseName,
            OfferingName = offering.Value.Name
        };

        Result<List<PeriodResponse>> periods = await _mediator.Send(new GetAllPeriodsQuery());

        if (periods.IsFailure)
            return View(viewModel);

        viewModel.Periods = new SelectList(periods.Value, "PeriodId", "Name", null, "Group");

        return View(viewModel);
    }
}
