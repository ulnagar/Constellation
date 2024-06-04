namespace Constellation.Application.StaffMembers.ReinstateStaffMember;

using Abstractions.Messaging;

public sealed record ReinstateStaffMemberCommand(
    string StaffId)
    : ICommand;