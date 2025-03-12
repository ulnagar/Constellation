namespace Constellation.Application.SchoolContacts.CreateContactRoleAssignment;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;

public sealed record CreateContactRoleAssignmentCommand(
    SchoolContactId ContactId,
    string SchoolCode,
    Position Position,
    string Note)
    : ICommand;