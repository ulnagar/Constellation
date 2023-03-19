#nullable enable
namespace Constellation.Application.Families.GetFamilyContactsForStudent;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;

public sealed record FamilyContactResponse(
    bool IsResidentialContact,
    Parent.SentralReference ContactType,
    string Name,
    EmailAddress? EmailAddress,
    PhoneNumber? MobileNumber,
    ParentId? ParentId,
    FamilyId? FamilyId);
