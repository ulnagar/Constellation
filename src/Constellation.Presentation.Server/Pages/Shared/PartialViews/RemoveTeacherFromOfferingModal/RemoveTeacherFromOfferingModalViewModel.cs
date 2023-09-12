namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveTeacherFromOfferingModal;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

internal sealed record RemoveTeacherFromOfferingModalViewModel(
    OfferingId OfferingId,
    string TeacherName,
    string AssignmentType,
    string CourseName,
    string OfferingName);