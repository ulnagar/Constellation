namespace Constellation.Application.Domains.StaffMembers.Commands.AddStaffToFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;

public sealed record AddStaffToFacultyCommand(
        string StaffId,
        FacultyId FacultyId,
        FacultyMembershipRole Role)
    : ICommand;