namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Core.ValueObjects;

public sealed record StaffResponse(
    string StaffId,
    Name Name,
    EmailAddress EmailAddress,
    string PortalUsername,
    string SchoolCode,
    bool IsShared);