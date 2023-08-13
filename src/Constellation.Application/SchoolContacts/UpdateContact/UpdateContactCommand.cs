namespace Constellation.Application.SchoolContacts.UpdateContact;

using Constellation.Application.Abstractions.Messaging;

public sealed record UpdateContactCommand(
    int ContactId,
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber)
    : ICommand;