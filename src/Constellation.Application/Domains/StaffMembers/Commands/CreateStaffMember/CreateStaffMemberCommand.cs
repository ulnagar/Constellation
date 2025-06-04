namespace Constellation.Application.Domains.StaffMembers.Commands.CreateStaffMember;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Enums;

public sealed record CreateStaffMemberCommand(
    string EmployeeId,
    string FirstName,
    string PreferredName,
    string LastName,
    Gender Gender,
    string EmailAddress,
    string SchoolCode,
    bool IsShared)
    : ICommand;