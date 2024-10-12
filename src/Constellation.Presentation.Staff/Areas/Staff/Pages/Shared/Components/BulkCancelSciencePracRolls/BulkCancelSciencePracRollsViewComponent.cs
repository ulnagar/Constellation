namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.BulkCancelSciencePracRolls;

using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class BulkCancelSciencePracRollsViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public BulkCancelSciencePracRollsViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        BulkCancelSciencePracRollsSelection viewModel = new();
        Result<List<SchoolSelectionListResponse>> result = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery());

        if (result.IsFailure)
        {
            // How? This method does not return failure
        }
        else
        {
            viewModel.Schools = result.Value;
        }

        return View(viewModel);
    }
}
