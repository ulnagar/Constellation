namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveStudentProvisionConfirmationModal;

using Application.Domains.Assessments.Provisions.Models;

public sealed record RemoveStudentProvisionConfirmationModalViewModel(StudentProvisionResponse Provision)
{
    public string ViewName = "RemoveStudentProvisionConfirmationModal";
}
