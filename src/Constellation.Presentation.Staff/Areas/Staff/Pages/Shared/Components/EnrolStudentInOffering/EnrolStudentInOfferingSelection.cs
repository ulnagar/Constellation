namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.EnrolStudentInOffering;

using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;

public class EnrolStudentInOfferingSelection
{
    public OfferingId OfferingId { get; set; }
    public string OfferingName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;

    public StudentId StudentId { get; set; } = StudentId.Empty;
    public Dictionary<StudentId, string> Students { get; set; } = new();
}
