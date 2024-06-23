namespace Constellation.Application.Training.GenerateModuleReport;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GenerateModuleReportCommand(
    TrainingModuleId Id,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
