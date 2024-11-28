namespace Constellation.Application.Attachments.PublishTemporaryFile;

using Abstractions.Messaging;
using Core.Models.Reports.Identifiers;

public sealed record PublishTemporaryFileCommand(
    ExternalReportId ReportId)
    : ICommand;
