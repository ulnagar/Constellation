namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.DisplayCaseActionDetails;

using Constellation.Core.Models.WorkFlow;
using Microsoft.AspNetCore.Mvc;

public class DisplayCaseActionDetailsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(Action action) =>
        action switch
        {
            null => Content(string.Empty),
            SendEmailAction emailAction => View("SendEmailAction", emailAction),
            CreateSentralEntryAction sentralAction => View("CreateSentralEntryAction", sentralAction),
            ConfirmSentralEntryAction confirmAction => View("ConfirmSentralEntryAction", confirmAction),
            PhoneParentAction phoneAction => View("PhoneParentAction", phoneAction),
            ParentInterviewAction interviewAction => View("ParentInterviewAction", interviewAction),
            CaseDetailUpdateAction updateAction => View("CaseDetailUpdateAction", updateAction),
            SentralIncidentStatusAction incidentAction => View("SentralIncidentStatusAction", incidentAction),
            UploadTrainingCertificateAction uploadAction => View("UploadTrainingCertificateAction", uploadAction),
            _ => Content(string.Empty)
        };
}