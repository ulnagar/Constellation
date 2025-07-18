﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffTrainingReport;

using Application.Domains.StaffMembers.Queries.GetStaffMembersAsDictionary;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Identifiers;
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
        Result<Dictionary<StaffId, string>> staffListRequest = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        if (staffListRequest.IsSuccess)
        {
            viewModel.StaffList = staffListRequest.Value;
        }

        return View(viewModel);
    }
}
