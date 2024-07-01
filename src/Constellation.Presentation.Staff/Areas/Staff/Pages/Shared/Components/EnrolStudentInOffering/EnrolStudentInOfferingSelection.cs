namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.EnrolStudentInOffering;

using Constellation.Core.Models.Offerings.Identifiers;

public class EnrolStudentInOfferingSelection
{
    public OfferingId OfferingId { get; set; }
    public string OfferingName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;
    public Dictionary<string, string> Students { get; set; } = new();
}
