namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContactRoleAssignment;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;

public sealed record CreateContactRoleAssignmentCommand(
    SchoolContactId ContactId,
    SchoolCode SchoolCode,
    Position Position,
    string Note)
    : ICommand;