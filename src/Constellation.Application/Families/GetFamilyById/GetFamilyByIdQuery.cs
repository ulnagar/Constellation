namespace Constellation.Application.Families.GetFamilyById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Families.Models;
using Constellation.Core.Models.Identifiers;

public sealed record GetFamilyByIdQuery(
    FamilyId FamilyId)
    : IQuery<FamilyResponse>;
