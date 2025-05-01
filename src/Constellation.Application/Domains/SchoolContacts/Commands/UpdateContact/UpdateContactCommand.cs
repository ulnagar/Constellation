namespace Constellation.Application.Domains.SchoolContacts.Commands.UpdateContact;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record UpdateContactCommand(
    SchoolContactId ContactId,
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber)
    : ICommand;