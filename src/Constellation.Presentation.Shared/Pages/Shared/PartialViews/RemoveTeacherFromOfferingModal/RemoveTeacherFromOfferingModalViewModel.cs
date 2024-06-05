namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.RemoveTeacherFromOfferingModal;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveTeacherFromOfferingModalViewModel(
    OfferingId OfferingId,
    string StaffId,
    string TeacherName,
    string AssignmentType,
    string CourseName,
    string OfferingName);