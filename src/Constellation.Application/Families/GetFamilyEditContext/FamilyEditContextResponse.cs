namespace Constellation.Application.Families.GetFamilyEditContext;

using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record FamilyEditContextResponse(
    FamilyId FamilyId,
    string FamilyTitle,
    string AddressLine1,
    string AddressLine2,
    string AddressTown,
    string AddressPostCode,
    string FamilyEmail,
    List<string> Students,
    List<string> Parents);