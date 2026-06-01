namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Core.Models.Common.Enums;
using Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.ValueObjects;
using Core.ValueObjects;

public sealed record StaffResponse(
    StaffId StaffId,
    EmployeeId EmployeeId,
    Name Name,
    Gender Gender,
    EmailAddress EmailAddress,
    PhoneNumber PhoneNumber,
    SchoolCode SchoolCode,
    bool IsShared);