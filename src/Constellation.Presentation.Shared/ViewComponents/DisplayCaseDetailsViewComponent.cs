namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Constellation.Core.Models.WorkFlow.Repositories;
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

        if (item.Detail is ComplianceCaseDetail complianceDetails)
            return View("ComplianceCaseDetail", complianceDetails);

        return Content(string.Empty);
    }
}