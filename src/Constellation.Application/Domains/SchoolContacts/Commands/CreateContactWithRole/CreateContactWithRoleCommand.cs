namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContactWithRole;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Enums;

public sealed record CreateContactWithRoleCommand(
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber,
    Position Position,
    string SchoolCode,
    string Note,
    bool SelfRegistered)
    : ICommand;
