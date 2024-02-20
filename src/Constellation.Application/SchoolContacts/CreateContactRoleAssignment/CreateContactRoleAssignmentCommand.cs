namespace Constellation.Application.SchoolContacts.CreateContactRoleAssignment;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record CreateContactRoleAssignmentCommand(
    SchoolContactId ContactId,
    string SchoolCode,
    string Position,
    string Note)
    : ICommand;