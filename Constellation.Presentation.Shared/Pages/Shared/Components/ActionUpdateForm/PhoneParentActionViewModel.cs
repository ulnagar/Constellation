namespace Constellation.Presentation.Shared.Pages.Shared.Components.ActionUpdateForm;

using Constellation.Application.Families.Models;

public sealed class PhoneParentActionViewModel
{
    public string ParentName { get; set; }
    public string ParentPhoneNumber { get; set; }
    public DateTime DateOccurred { get; set; }
    public int IncidentNumber { get; set; }

    public List<FamilyContactResponse> Parents { get; set; }
}
