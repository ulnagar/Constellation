namespace Constellation.Application.Training.Modules.GenerateStaffReport;

using Constellation.Application.Abstractions.Messaging;
using Models;

public sealed record GenerateStaffReportCommand(
    string StaffId,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
