namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddSessionToTutorial;

using Constellation.Application.Domains.StaffMembers.Models;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Application.Domains.Timetables.Periods.Queries.GetAllPeriods;
using Constellation.Core.Shared;
using Core.Models.Timetables.Enums;
using Core.Models.Tutorials.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AddSessionToTutorialViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddSessionToTutorialViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(TutorialId id)
    {

        Result<List<StaffSelectionListResponse>> staffResult = await _mediator.Send(new GetStaffForSelectionListQuery());

        AddSessionToTutorialSelection viewModel = new()
        {
            TutorialId = id
        };

        foreach (StaffSelectionListResponse staff in staffResult.Value.OrderBy(staff => staff.Name.LastName))
        {
            viewModel.Staff.Add(staff.StaffId, staff.Name.DisplayName);
        }

        Result<List<PeriodResponse>> periods = await _mediator.Send(new GetAllPeriodsQuery());

        if (periods.IsFailure)
            return View(viewModel);

        viewModel.Periods = new SelectList(periods.Value.OrderBy(period => period.SortOrder), nameof(PeriodResponse.PeriodId), nameof(PeriodResponse.Name), null, nameof(PeriodResponse.Group));

        return View(viewModel);
    }

}
