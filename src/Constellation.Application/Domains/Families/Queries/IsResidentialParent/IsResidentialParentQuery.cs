namespace Constellation.Application.Domains.Families.Queries.IsResidentialParent;

using Abstractions.Messaging;

public sealed record IsResidentialParentQuery(
    string ParentEmail)
    : IQuery<bool>;
