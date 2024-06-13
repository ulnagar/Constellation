namespace Constellation.Presentation.Shared.ViewComponents;

using Application.Schools.GetSchoolsForSelectionList;
using Application.Schools.Models;
using Application.StaffMembers.GetStaffForSelectionList;
using Application.StaffMembers.Models;
using Application.Students.GetCurrentStudentsAsDictionary;
using Core.Models.Assets.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pages.Shared.Components.AllocateAsset;

public class AllocateAssetViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AllocateAssetViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(AssetId assetId)
    {
        AllocateDeviceSelection viewModel = new();

        Result<Dictionary<string, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (students.IsFailure)
        {
            return Content(string.Empty);
        }

        viewModel.StudentList = new SelectList(students.Value, "Key", "Value");

        Result<List<StaffSelectionListResponse>> staff = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staff.IsFailure)
        {
            return Content(string.Empty);
        }

        viewModel.StaffList = new SelectList(staff.Value, "Key", "Value");

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        if (schools.IsFailure)
        {
            return Content(string.Empty);
        }

        viewModel.SchoolList = new SelectList(schools.Value, "Code", "Name");

        return View();
    }
}