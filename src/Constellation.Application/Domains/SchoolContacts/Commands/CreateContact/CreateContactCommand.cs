namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContact;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;

public sealed record CreateContactCommand(
    string FirstName,
    string LastName,
    EmailAddress EmailAddress,
    PhoneNumber PhoneNumber,
    bool SelfRegistered)
    : ICommand<SchoolContactId>;
