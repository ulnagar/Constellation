namespace Constellation.Application.Domains.SchoolContacts.Queries.DoesEmailBelongToSchoolContact;

using Abstractions.Messaging;

public sealed record DoesEmailBelongToSchoolContactQuery(
    string EmailAddress)
    : IQuery<bool>;