namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.ConfirmTempReportPublishModal;

using Core.Models.Reports.Identifiers;

public sealed record ConfirmTempReportPublishModalViewModel(
    ExternalReportId ReportId,
    string FileName);