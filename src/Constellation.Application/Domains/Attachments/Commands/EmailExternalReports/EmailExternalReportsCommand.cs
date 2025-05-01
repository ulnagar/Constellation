namespace Constellation.Application.Domains.Attachments.Commands.EmailExternalReports;

using Abstractions.Messaging;

public sealed record EmailExternalReportsCommand(
    string Subject,
    string Body)
    : ICommand;