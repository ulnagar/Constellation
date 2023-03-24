#nullable enable
namespace Constellation.Application.Families.Models;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;

public sealed record FamilyContactResponse(
    bool IsResidentialContact,
    Parent.SentralReference ContactType,
    string Name,
    EmailAddress? EmailAddress,
    PhoneNumber? MobileNumber,
    ParentId? ParentId,
    FamilyId? FamilyId,
    List<string> Students);
