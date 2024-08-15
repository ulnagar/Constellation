namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffTrainingReport;

using Constellation.Application.Features.Common.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class StaffTrainingReportViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public StaffTrainingReportViewComponent(ISender mediator)
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
