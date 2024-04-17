namespace Constellation.Presentation.Server.Pages.Shared.Components.ActionUpdateForm;

using Constellation.Application.Families.Models;
using Core.Models.WorkFlow.Identifiers;

public sealed class ParentInterviewActionViewModel
{
    public ActionId ActionId { get; set; }
    public List<Attendee> Attendees { get; set; } = new();
    public DateTime DateOccurred { get; set; }
    public int IncidentNumber { get; set; }
    public List<FamilyContactResponse> Parents { get; set; } = new();

    public sealed class Attendee
    {
        public string Name { get; set; }
        public string Notes { get; set; }
    }
}
