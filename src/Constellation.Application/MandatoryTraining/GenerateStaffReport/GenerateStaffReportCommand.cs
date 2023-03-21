namespace Constellation.Application.MandatoryTraining.GenerateStaffReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;

public sealed record GenerateStaffReportCommand(
    string StaffId,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
