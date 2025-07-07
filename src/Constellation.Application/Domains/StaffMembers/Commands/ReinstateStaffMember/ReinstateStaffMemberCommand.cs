namespace Constellation.Application.Domains.StaffMembers.Commands.ReinstateStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record ReinstateStaffMemberCommand(
    StaffId StaffId,
    string SchoolCode)
    : ICommand;