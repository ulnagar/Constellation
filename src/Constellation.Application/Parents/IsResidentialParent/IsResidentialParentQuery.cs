namespace Constellation.Application.Parents.IsResidentialParent;

using Constellation.Application.Abstractions.Messaging;

public sealed record IsResidentialParentQuery(
    string ParentEmail)
    : IQuery<bool>;
