namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.ConfirmRemoveDocumentFromAssessmentModal;

using Core.Models.Assessments.Identifiers;

public sealed record ConfirmRemoveDocumentFromAssessmentModalViewModel(
    AssessmentDownloadId DocumentId,
    string DocumentName);
