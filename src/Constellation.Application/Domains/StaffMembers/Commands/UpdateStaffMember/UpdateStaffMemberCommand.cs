namespace Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Enums;

public sealed record UpdateStaffMemberCommand(
    StaffId StaffId,
    string EmployeeId,
    string FirstName,
    string PreferredName,
    string LastName,
    Gender Gender,
    string EmailAddress,
    string SchoolCode,
    bool IsShared)
    : ICommand;