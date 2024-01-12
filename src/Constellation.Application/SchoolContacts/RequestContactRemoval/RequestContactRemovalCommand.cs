namespace Constellation.Application.SchoolContacts.RequestContactRemoval;

using Abstractions.Messaging;

public sealed record RequestContactRemovalCommand(
    int AssignmentId,
    string Comment,
    string CancelledBy,
    string CancelledAt)
    : ICommand;