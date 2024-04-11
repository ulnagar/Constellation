namespace Constellation.Presentation.Server.ViewComponents;

using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Core.Models.WorkFlow.Repositories;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.ActionUpdateForm;

public class ActionUpdateFormViewComponent : ViewComponent
{
    private readonly ICaseRepository _caseRepository;

    public ActionUpdateFormViewComponent(
        ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid caseId, Guid actionId)
    {
        Case item = await _caseRepository.GetById(CaseId.FromValue(caseId));

        if (item is null)
            return Content(string.Empty);

        Action action = item.Actions.FirstOrDefault(action => action.Id == ActionId.FromValue(actionId));

        if (action is null)
            return Content(string.Empty);

        switch (action)
        {
            case SendEmailAction emailAction:
                break;

            case CreateSentralEntryAction sentralAction:
                CreateSentralEntryActionViewModel viewModel = new();

                return View("CreateSentralEntryAction", viewModel);

            case ConfirmSentralEntryAction confirmAction:
                break;

            case PhoneParentAction phoneAction:
                break;

            case ParentInterviewAction interviewAction:
                break;

            default:
                return Content(string.Empty);
        };

        return Content(string.Empty);
    }
}