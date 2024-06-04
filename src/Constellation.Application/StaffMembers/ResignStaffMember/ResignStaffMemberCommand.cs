namespace Constellation.Application.StaffMembers.ResignStaffMember;

using Abstractions.Messaging;

public sealed record ResignStaffMemberCommand(
    string StaffId)
    : ICommand;