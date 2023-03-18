#nullable enable
namespace Constellation.Application.Families.GetFamilyContactsForStudent;

using Constellation.Core.Models.Families;
using Constellation.Core.ValueObjects;
using System;

public sealed record FamilyContactResponse(
    bool IsResidentialContact,
    Parent.SentralReference ContactType,
    string Name,
    EmailAddress? EmailAddress,
    PhoneNumber? MobileNumber,
    Guid? ParentId,
    Guid? FamilyId);
