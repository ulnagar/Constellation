namespace Constellation.Application.Domains.StaffMembers.Commands.ResignStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record ResignStaffMemberCommand(
    StaffId StaffId)
    : ICommand;