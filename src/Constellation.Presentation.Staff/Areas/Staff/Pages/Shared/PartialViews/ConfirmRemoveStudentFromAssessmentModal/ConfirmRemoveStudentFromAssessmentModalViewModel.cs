namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.ConfirmRemoveStudentFromAssessmentModal;

using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record ConfirmRemoveStudentFromAssessmentModalViewModel(
    StudentId StudentId,
    Name Student);
