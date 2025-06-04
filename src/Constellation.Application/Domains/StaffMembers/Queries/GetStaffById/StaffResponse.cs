namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;

public sealed record StaffResponse(
    StaffId StaffId,
    Name Name,
    EmailAddress EmailAddress,
    string SchoolCode,
    bool IsShared);