namespace Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMember;

using Abstractions.Messaging;

public sealed record UpdateStaffMemberCommand(
    string StaffId,
    string FirstName,
    string LastName,
    string PortalUsername,
    string SchoolCode,
    bool IsShared)
    : ICommand;