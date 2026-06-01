namespace Constellation.Application.Domains.StaffMembers.Commands.CreateStaffMember;

using Abstractions.Messaging;
using Core.Models.Common.Enums;
using Core.Models.Identifiers;

public sealed record CreateStaffMemberCommand(
    string EmployeeId,
    string FirstName,
    string PreferredName,
    string LastName,
    Gender Gender,
    string EmailAddress,
    SchoolCode SchoolCode,
    bool IsShared)
    : ICommand;