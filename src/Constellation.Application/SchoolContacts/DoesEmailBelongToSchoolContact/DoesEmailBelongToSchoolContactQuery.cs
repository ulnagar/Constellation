namespace Constellation.Application.SchoolContacts.DoesEmailBelongToSchoolContact;

using Abstractions.Messaging;

public sealed record DoesEmailBelongToSchoolContactQuery(
    string EmailAddress)
    : IQuery<bool>;