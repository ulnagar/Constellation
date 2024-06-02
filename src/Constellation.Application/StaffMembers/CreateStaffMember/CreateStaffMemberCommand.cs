namespace Constellation.Application.StaffMembers.CreateStaffMember;

using Abstractions.Messaging;

public sealed record CreateStaffMemberCommand(
    string StaffId,
    string FirstName,
    string LastName,
    string PortalUsername,
    string SchoolCode,
    bool IsShared)
    : ICommand;