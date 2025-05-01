namespace Constellation.Application.Domains.SciencePracs.Queries.GenerateOverdueReport;

using Abstractions.Messaging;
using DTOs;

public sealed record GenerateOverdueReportCommand()
    : ICommand<FileDto>;