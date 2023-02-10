namespace Constellation.Application.Staff.GetStaffForSelectionList;

public sealed record StaffSelectionListResponse(
    string StaffId,
    string FirstName,
    string LastName);
