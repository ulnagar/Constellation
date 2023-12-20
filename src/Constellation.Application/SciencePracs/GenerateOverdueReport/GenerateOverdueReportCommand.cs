namespace Constellation.Application.SciencePracs.GenerateOverdueReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;

public sealed record GenerateOverdueReportCommand()
    : ICommand<FileDto>;