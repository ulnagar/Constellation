﻿namespace Constellation.Presentation.Server.ViewComponents;

using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Core.Models.WorkFlow.Repositories;
using Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;

public class DisplayCaseDetailsViewComponent : ViewComponent
{
    private readonly ICaseRepository _caseRepository;

    public DisplayCaseDetailsViewComponent(
        ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync([ModelBinder(typeof(StrongIdBinder))] CaseId caseId)
    {
        Case item = await _caseRepository.GetById(caseId);

        if (item is null)
            return Content(string.Empty);

        if (item.Detail is AttendanceCaseDetail attendanceDetails)
            return View("AttendanceCaseDetail", attendanceDetails);

        return Content(string.Empty);
    }
}