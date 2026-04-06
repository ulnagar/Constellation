namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContactWithRole;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts.Enums;
using Core.ValueObjects;

public sealed record CreateContactWithRoleCommand(
    string FirstName,
    string LastName,
    EmailAddress EmailAddress,
    PhoneNumber PhoneNumber,
    Position Position,
    SchoolCode SchoolCode,
    string Note,
    bool SelfRegistered)
    : ICommand;
