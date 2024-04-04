namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Core.Models.WorkFlow;
using Microsoft.AspNetCore.Mvc;

public class DisplayCaseActionDetailsViewComponent : ViewComponent
{
    public DisplayCaseActionDetailsViewComponent()
    {
    }

    public async Task<IViewComponentResult> InvokeAsync(Action action)
    {
        if (action is null)
            return Content(string.Empty);

        if (action is SendEmailAction emailAction)
            return View("SendEmailAction", emailAction);

        if (action is CreateSentralEntryAction sentralAction)
            return View("CreateSentralEntryAction", sentralAction);

        if (action is ConfirmSentralEntryAction confirmAction)
            return View("ConfirmSentralEntryAction", confirmAction);

        if (action is PhoneParentAction phoneAction)
            return View("PhoneParentAction", phoneAction);

        if (action is ParentInterviewAction interviewAction)
            return View("ParentInterviewAction", interviewAction);

        return Content(string.Empty);
    }
}