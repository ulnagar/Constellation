namespace Constellation.Application.MandatoryTraining.GenerateModuleReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public sealed record GenerateModuleReportCommand(
    TrainingModuleId Id,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
