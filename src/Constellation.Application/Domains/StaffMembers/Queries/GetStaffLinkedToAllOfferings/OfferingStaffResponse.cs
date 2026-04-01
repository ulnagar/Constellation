namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffLinkedToAllOfferings;

using Core.Models.Offerings.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;

public sealed record OfferingStaffResponse(
    OfferingId OfferingId,
    StaffId StaffId,
    Name Name);
