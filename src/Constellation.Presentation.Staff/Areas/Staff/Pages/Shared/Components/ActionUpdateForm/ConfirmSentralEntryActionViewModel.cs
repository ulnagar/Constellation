namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ActionUpdateForm;

public sealed class ConfirmSentralEntryActionViewModel
{
    public const string AttendanceWarning =
        "By marking this action as Confirmed you are affirming that the classroom teacher has correctly created a Sentral Incident for outstanding work in the indicated class, or that such an entry was not required.";

    public const string ComplianceWarning =
        "By marking this action as Confirmed you are affirming that the classroom teacher has correctly reported the status of the Sentral Incident above, and completed any further actions required.";

    public bool Confirmed { get; set; }
    public string Warning { get; set; }
}