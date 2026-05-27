namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ExtendTutorial;

using Constellation.Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class ExtendTutorialViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public ExtendTutorialViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(DateOnly endDate)
    {
        ExtendTutorialSelection viewModel = new();

        Result<List<SchoolCalendarWeek>> validDates = await _mediator.Send(new GetTermsAndWeeksForCurrentYearQuery());

        if (validDates.IsFailure)
            return Content(string.Empty);

        viewModel.ValidEndDates = validDates.Value
            .Where(entry => entry.EndDate > endDate.ToDateTime(TimeOnly.MinValue))
            .OrderBy(entry => entry.EndDate)
            .ToList();

        return View(viewModel);
    }
}
