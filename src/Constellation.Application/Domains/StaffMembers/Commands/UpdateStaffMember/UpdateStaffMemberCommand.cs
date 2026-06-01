namespace Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMember;

using Abstractions.Messaging;
using Core.Models.Common.Enums;
using Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;

public sealed record UpdateStaffMemberCommand(
    StaffId StaffId,
    string EmployeeId,
    string FirstName,
    string PreferredName,
    string LastName,
    Gender Gender,
    string EmailAddress,
    SchoolCode SchoolCode,
    bool IsShared)
    : ICommand;