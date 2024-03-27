namespace Constellation.Application.Families.GetFamilyEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetFamilyEditContextQuery(
    FamilyId FamilyId)
    : IQuery<FamilyEditContextResponse>;
