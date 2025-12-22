namespace Constellation.Application.Domains.SchoolContacts.Commands.UpdateSchoolContactPhoneNumber;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;

public sealed record UpdateSchoolContactPhoneNumberCommand(
    SchoolContactId ContactId,
    PhoneNumber PhoneNumber)
    : ICommand;
