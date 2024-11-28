namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.ConfirmTempReportDeleteModal;

using Core.Models.Reports.Identifiers;

public sealed record ConfirmTempReportDeleteModalViewModel(
    ExternalReportId ReportId,
    string FileName);