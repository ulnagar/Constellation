namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AllocateAsset;

using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Application.StaffMembers.GetStaffForSelectionList;
using Constellation.Application.StaffMembers.Models;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AllocateAssetViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AllocateAssetViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        AllocateDeviceSelection viewModel = new();

        Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (students.IsFailure)
            return Content(string.Empty);

        viewModel.StudentList = new SelectList(students.Value, "Key", "Value");

        Result<List<StaffSelectionListResponse>> staff = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staff.IsFailure)
            return Content(string.Empty);

        viewModel.StaffList = new SelectList(staff.Value.OrderBy(entry => entry.LastName), "StaffId", "DisplayName");

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        if (schools.IsFailure)
            return Content(string.Empty);

        viewModel.SchoolList = new SelectList(schools.Value.OrderBy(entry => entry.Name), "Code", "Name");

        return View(viewModel);
    }
}