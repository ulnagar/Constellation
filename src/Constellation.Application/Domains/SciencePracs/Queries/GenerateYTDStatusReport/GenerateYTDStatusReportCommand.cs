namespace Constellation.Application.Domains.SciencePracs.Queries.GenerateYTDStatusReport;

using Abstractions.Messaging;
using DTOs;

public sealed record GenerateYTDStatusReportCommand
    : ICommand<FileDto>;