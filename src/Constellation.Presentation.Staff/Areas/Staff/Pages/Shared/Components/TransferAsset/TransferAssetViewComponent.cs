namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TransferAsset;

using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TransferAssetViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _dateTime;

    public TransferAssetViewComponent(
        ISender mediator,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _dateTime = dateTime;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        TransferAssetSelection viewModel = new();

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        if (schools.IsFailure)
            return Content(string.Empty);

        viewModel.SchoolList = new SelectList(schools.Value.OrderBy(entry => entry.Name), "Code", "Name");

        viewModel.ArrivalDate = _dateTime.Today;

        return View(viewModel);
    }
}