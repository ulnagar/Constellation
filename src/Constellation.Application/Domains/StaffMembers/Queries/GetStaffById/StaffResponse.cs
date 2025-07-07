namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.Students.Enums;
using Core.ValueObjects;

public sealed record StaffResponse(
    StaffId StaffId,
    EmployeeId EmployeeId,
    Name Name,
    Gender Gender,
    EmailAddress EmailAddress,
    string SchoolCode,
    bool IsShared);