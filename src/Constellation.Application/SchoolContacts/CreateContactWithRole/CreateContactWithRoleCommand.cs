namespace Constellation.Application.SchoolContacts.CreateContactWithRole;

using Constellation.Application.Abstractions.Messaging;

public sealed record CreateContactWithRoleCommand(
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber,
    string Position,
    string SchoolCode,
    bool SelfRegistered)
    : ICommand;
