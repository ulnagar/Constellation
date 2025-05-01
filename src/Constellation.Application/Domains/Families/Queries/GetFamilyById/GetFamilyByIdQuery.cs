namespace Constellation.Application.Domains.Families.Queries.GetFamilyById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Families.Models;
using Constellation.Core.Models.Identifiers;

public sealed record GetFamilyByIdQuery(
    FamilyId FamilyId)
    : IQuery<FamilyResponse>;
