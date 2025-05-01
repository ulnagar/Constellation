namespace Constellation.Application.Domains.Attachments.Commands.DeleteTemporaryFile;

using Abstractions.Messaging;
using Core.Models.Reports.Identifiers;

public sealed record DeleteTemporaryFileCommand(
    ExternalReportId ReportId)
    : ICommand;