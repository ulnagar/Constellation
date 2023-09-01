namespace Constellation.Application.StaffMembers.GetStaffById;

using Constellation.Core.ValueObjects;

public sealed record StaffResponse(
    string StaffId,
    Name Name,
    EmailAddress EmailAddress);