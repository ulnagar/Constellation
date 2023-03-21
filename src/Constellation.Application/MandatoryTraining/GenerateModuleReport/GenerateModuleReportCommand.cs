namespace Constellation.Application.MandatoryTraining.GenerateModuleReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Core.Models.Identifiers;

public sealed record GenerateModuleReportCommand(
    TrainingModuleId Id,
    bool IncludeCertificates)
    : ICommand<ReportDto>;
