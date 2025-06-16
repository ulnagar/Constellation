namespace Constellation.Application.Domains.StaffMembers.Commands.RemoveSchoolAssignment;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record RemoveSchoolAssignmentCommand(
    StaffId StaffId,
    SchoolAssignmentId AssignmentId)
    : ICommand;
