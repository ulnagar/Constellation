namespace Constellation.Application.Domains.StaffMembers.Models;

public sealed record StaffSelectionListResponse(
    string StaffId,
    string FirstName,
    string LastName)
{
    public string DisplayName => $"{FirstName} {LastName}";
}
