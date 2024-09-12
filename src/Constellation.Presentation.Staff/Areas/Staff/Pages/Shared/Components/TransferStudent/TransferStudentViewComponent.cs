namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TransferStudent;

using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;

public sealed class TransferStudentViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _dateTime;

    public TransferStudentViewComponent(
        ISender mediator,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _dateTime = dateTime;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.All));

        if (schools.IsFailure)
            return Content(string.Empty);

        TransferStudentSelection viewModel = new()
        {
            SchoolList = new SelectList(schools.Value.OrderBy(entry => entry.Name), "Code", "Name"),
            StartDate = _dateTime.Today
        };

        return View(viewModel);
    }
}