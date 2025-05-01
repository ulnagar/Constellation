namespace Constellation.Application.Domains.Training.Queries.GenerateOverallReport;

using Abstractions.Messaging;
using DTOs;

public sealed record GenerateOverviewReportCommand
    : ICommand<FileDto>;