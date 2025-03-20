namespace Constellation.Application.SciencePracs.GenerateYTDStatusReport;

using Abstractions.Messaging;
using DTOs;

public sealed record GenerateYTDStatusReportCommand
    : ICommand<FileDto>;