namespace Constellation.Application.SchoolContacts.CreateContactRoleAssignment;

using Constellation.Application.Abstractions.Messaging;

public sealed record CreateContactRoleAssignmentCommand(
    int ContactId,
    string SchoolCode,
    string Position)
    : ICommand;