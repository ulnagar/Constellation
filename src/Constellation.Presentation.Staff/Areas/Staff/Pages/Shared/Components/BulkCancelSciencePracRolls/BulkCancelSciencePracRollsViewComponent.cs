namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.BulkCancelSciencePracRolls;

using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Core.Shared;
using Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        viewModel.GradeList = new SelectList(Grade.GetOptions.Where(grade => grade.Order > 0), nameof(Grade.Value),
            nameof(Grade.Name));

        return View(viewModel);
    }
}
