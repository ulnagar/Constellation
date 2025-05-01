namespace Constellation.Application.Domains.Attachments.Commands.PublishTemporaryFile;

using Abstractions.Messaging;
using Core.Models.Reports.Identifiers;

public sealed record PublishTemporaryFileCommand(
    ExternalReportId ReportId)
    : ICommand;
