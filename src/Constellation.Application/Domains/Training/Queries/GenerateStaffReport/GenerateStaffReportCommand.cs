namespace Constellation.Application.Domains.Training.Queries.GenerateStaffReport;

using Abstractions.Messaging;
using Models;

public sealed record GenerateStaffReportCommand(
    string StaffId,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
