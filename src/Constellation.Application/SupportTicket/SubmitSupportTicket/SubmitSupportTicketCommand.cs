namespace Constellation.Application.SupportTicket.SubmitSupportTicket;

using Abstractions.Messaging;
using Core.ValueObjects;

public sealed record SubmitSupportTicketCommand(
    EmailRecipient Submitter,
    string IssueType,
    string DeviceIdentifier,
    string Description)
    : ICommand;