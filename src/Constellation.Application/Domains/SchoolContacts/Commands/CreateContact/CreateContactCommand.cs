namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContact;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record CreateContactCommand(
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber,
    bool SelfRegistered)
    : ICommand<SchoolContactId>;
