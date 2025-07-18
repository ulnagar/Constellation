﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddTeacherToOffering;

using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingDetails;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Identifiers;
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

    public async Task<IViewComponentResult> InvokeAsync(OfferingId id)
    {
        Result<OfferingDetailsResponse> offering = await _mediator.Send(new GetOfferingDetailsQuery(id));
        Result<List<StaffSelectionListResponse>> staffResult = await _mediator.Send(new GetStaffForSelectionListQuery());

        Dictionary<StaffId, string> staffList = new();

        foreach (StaffSelectionListResponse staff in staffResult.Value.OrderBy(staff => staff.Name.LastName))
        {
            staffList.Add(staff.StaffId, staff.Name.DisplayName);
        }

        Dictionary<string, string> resourceList = new();

        foreach (AssignmentType entry in AssignmentType.Enumerations())
        {
            resourceList.Add(entry.Value, entry.Value);
        }

        AddTeacherToOfferingSelection viewModel = new()
        {
            OfferingId = id,
            CourseName = offering.Value.CourseName,
            OfferingName = offering.Value.Name,
            Staff = staffList,
            AssignmentTypes = resourceList
        };

        return View(viewModel);
    }
}
