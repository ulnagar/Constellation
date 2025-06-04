namespace Constellation.Application.Domains.StaffMembers.Commands.AddStaffToFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers.Identifiers;

public sealed record AddStaffToFacultyCommand(
        StaffId StaffId,
        FacultyId FacultyId,
        FacultyMembershipRole Role)
    : ICommand;