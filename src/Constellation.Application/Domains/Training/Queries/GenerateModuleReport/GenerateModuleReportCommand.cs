namespace Constellation.Application.Domains.Training.Queries.GenerateModuleReport;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GenerateModuleReportCommand(
    TrainingModuleId Id,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
