namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Application.Features.Common.Queries;
using Constellation.Presentation.Server.Pages.Shared.Components.StaffTrainingReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class StaffTrainingReportViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public StaffTrainingReportViewComponent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var viewModel = new StaffTrainingReportSelection();
        viewModel.StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        return View(viewModel);
    }
}
