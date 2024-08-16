namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.DisplayCaseDetails;

using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Constellation.Core.Models.WorkFlow.Repositories;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;

public class DisplayCaseDetailsViewComponent : ViewComponent
{
    private readonly ICaseRepository _caseRepository;

    public DisplayCaseDetailsViewComponent(
        ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync([ModelBinder(typeof(ConstructorBinder))] CaseId caseId)
    {
        Case? item = await _caseRepository.GetById(caseId);

        if (item is null)
            return Content(string.Empty);

        return item.Detail switch
        {
            null => Content(string.Empty),
            AttendanceCaseDetail attendanceDetails => View("AttendanceCaseDetail", attendanceDetails),
            ComplianceCaseDetail complianceDetails => View("ComplianceCaseDetail", complianceDetails),
            TrainingCaseDetail trainingDetails => View("TrainingCaseDetail", trainingDetails),
            _ => Content(string.Empty)
        };
    }
}