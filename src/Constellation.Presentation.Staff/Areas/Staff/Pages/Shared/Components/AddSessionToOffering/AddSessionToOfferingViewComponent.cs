namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddSessionToOffering;

using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Periods.GetAllPeriods;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AddSessionToOfferingViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddSessionToOfferingViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(OfferingId id)
    {
        Result<OfferingDetailsResponse> offering = await _mediator.Send(new GetOfferingDetailsQuery(id));

        AddSessionToOfferingSelection viewModel = new()
        {
            OfferingId = id,
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