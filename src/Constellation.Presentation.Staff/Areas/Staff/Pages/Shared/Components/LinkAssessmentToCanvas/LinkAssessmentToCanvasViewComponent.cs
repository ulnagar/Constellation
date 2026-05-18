namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.LinkAssessmentToCanvas;

using MediatR;
using Microsoft.AspNetCore.Mvc;

public sealed class LinkAssessmentToCanvasViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public LinkAssessmentToCanvasViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        LinkAssessmentToCanvasSelection viewModel = new();

        return View(viewModel);
    }
}
