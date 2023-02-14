namespace Constellation.Application.StaffMembers.GetStaffForSelectionList;

public sealed record StaffSelectionListResponse(
    string StaffId,
    string FirstName,
    string LastName);
