namespace Constellation.Application.Families.GetParentEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetParentEditContextQuery(
    FamilyId FamilyId,
    ParentId ParentId)
    : IQuery<ParentEditContextResponse>;
