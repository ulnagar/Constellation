namespace Constellation.Presentation.Shared.Pages.Shared.Components.AddTeacherToOffering;

using Constellation.Core.Models.Offerings.Identifiers;

public class AddTeacherToOfferingSelection
{
    public OfferingId OfferingId { get; set; }
    public string StaffId { get; set; }
    public string AssignmentType { get; set; }

    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    public Dictionary<string, string> Staff { get; set; } = new();
    public Dictionary<string, string> AssignmentTypes { get; set; } = new();
}
