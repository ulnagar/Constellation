namespace Constellation.Application.Domains.Families.Queries.DoesEmailBelongToParentOrFamily;

using Abstractions.Messaging;

public sealed record DoesEmailBelongToParentOrFamilyQuery(
    string EmailAddress)
    : IQuery<bool>;
