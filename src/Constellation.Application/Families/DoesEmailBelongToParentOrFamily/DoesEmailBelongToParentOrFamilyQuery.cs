namespace Constellation.Application.Families.DoesEmailBelongToParentOrFamily;

using Abstractions.Messaging;

public sealed record DoesEmailBelongToParentOrFamilyQuery(
    string EmailAddress)
    : IQuery<bool>;
