namespace Constellation.Application.SchoolContacts.CreateContact;

using Constellation.Application.Abstractions.Messaging;

public sealed record CreateContactCommand(
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber,
    bool SelfRegistered)
    : ICommand<int>;
