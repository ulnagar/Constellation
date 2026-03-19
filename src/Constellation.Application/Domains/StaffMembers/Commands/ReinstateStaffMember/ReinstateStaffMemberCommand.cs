namespace Constellation.Application.Domains.StaffMembers.Commands.ReinstateStaffMember;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;

public sealed record ReinstateStaffMemberCommand(
    StaffId StaffId,
    SchoolCode SchoolCode)
    : ICommand;