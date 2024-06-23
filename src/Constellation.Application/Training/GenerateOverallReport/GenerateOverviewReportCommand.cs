namespace Constellation.Application.Training.GenerateOverallReport;

using Abstractions.Messaging;
using DTOs;

public sealed record GenerateOverviewReportCommand
    : ICommand<FileDto>;