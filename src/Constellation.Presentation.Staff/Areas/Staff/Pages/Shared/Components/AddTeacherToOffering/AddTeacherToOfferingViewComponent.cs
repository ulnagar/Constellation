namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddTeacherToOffering;

using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.StaffMembers.GetStaffForSelectionList;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class AddTeacherToOfferingViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddTeacherToOfferingViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid Id)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);
        Result<OfferingDetailsResponse> offering = await _mediator.Send(new GetOfferingDetailsQuery(offeringId));
        Result<List<StaffSelectionListResponse>> staffResult = await _mediator.Send(new GetStaffForSelectionListQuery());

        Dictionary<string, string> staffList = new();

        foreach (StaffSelectionListResponse staff in staffResult.Value.OrderBy(staff => staff.LastName))
        {
            staffList.Add(staff.StaffId, $"{staff.FirstName} {staff.LastName}");
        }

        Dictionary<string, string> resourceList = new();

        foreach (AssignmentType entry in AssignmentType.Enumerations())
        {
            resourceList.Add(entry.Value, entry.Value);
        }

        var viewModel = new AddTeacherToOfferingSelection
        {
            OfferingId = offeringId,
            CourseName = offering.Value.CourseName,
            OfferingName = offering.Value.Name,
            Staff = staffList,
            AssignmentTypes = resourceList
        };

        return View(viewModel);
    }
}
