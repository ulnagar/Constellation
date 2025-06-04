namespace Constellation.Application.Domains.Training.Queries.GenerateStaffReport;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Models;

public sealed record GenerateStaffReportCommand(
    StaffId StaffId,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
