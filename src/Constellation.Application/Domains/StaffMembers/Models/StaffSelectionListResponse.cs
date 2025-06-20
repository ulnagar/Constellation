namespace Constellation.Application.Domains.StaffMembers.Models;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.ValueObjects;
using Core.ValueObjects;

public sealed record StaffSelectionListResponse(
    StaffId StaffId,
    EmployeeId EmployeeId,
    Name Name)
{
    public string DisplayName => Name.DisplayName;
}
