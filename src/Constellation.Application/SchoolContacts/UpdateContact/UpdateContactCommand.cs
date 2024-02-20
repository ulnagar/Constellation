namespace Constellation.Application.SchoolContacts.UpdateContact;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record UpdateContactCommand(
    SchoolContactId ContactId,
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber)
    : ICommand;