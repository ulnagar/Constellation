namespace Constellation.Application.Domains.SchoolContacts.Commands.UpdateContact;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;

public sealed record UpdateContactCommand(
    SchoolContactId ContactId,
    string FirstName,
    string LastName,
    EmailAddress EmailAddress,
    PhoneNumber PhoneNumber)
    : ICommand;