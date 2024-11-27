namespace Constellation.Application.Attachments.EmailExternalReports;

using Abstractions.Messaging;

public sealed record EmailExternalReportsCommand(
    string Subject,
    string Body)
    : ICommand;