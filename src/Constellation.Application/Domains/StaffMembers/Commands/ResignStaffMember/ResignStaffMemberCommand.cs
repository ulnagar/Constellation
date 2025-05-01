namespace Constellation.Application.Domains.StaffMembers.Commands.ResignStaffMember;

using Abstractions.Messaging;

public sealed record ResignStaffMemberCommand(
    string StaffId)
    : ICommand;