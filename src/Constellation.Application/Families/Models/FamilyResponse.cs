namespace Constellation.Application.Families.Models;

using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record FamilyResponse(
    FamilyId FamilyId,
    string FamilyName,
    List<ParentResponse> Parents);

