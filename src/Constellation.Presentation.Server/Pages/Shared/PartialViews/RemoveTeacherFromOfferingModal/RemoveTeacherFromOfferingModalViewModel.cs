namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveTeacherFromOfferingModal;

using Constellation.Core.Models.Offerings.Identifiers;

internal sealed record RemoveTeacherFromOfferingModalViewModel(
    OfferingId OfferingId,
    string StaffId,
    string TeacherName,
    string AssignmentType,
    string CourseName,
    string OfferingName);