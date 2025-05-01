namespace Constellation.Application.Domains.StaffMembers.Commands.ReinstateStaffMember;

using Abstractions.Messaging;

public sealed record ReinstateStaffMemberCommand(
    string StaffId)
    : ICommand;