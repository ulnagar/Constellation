namespace Constellation.Application.StaffMembers.AddStaffToFaculty;

using Abstractions.Messaging;
using Core.Models.Faculty.Identifiers;
using Core.Models.Faculty.ValueObjects;

public sealed record AddStaffToFacultyCommand(
        string StaffId,
        FacultyId FacultyId,
        FacultyMembershipRole Role)
    : ICommand;