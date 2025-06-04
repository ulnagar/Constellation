namespace Constellation.Application.Domains.StaffMembers.Commands.RemoveStaffFromFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;
using Core.Models.StaffMembers.Identifiers;

public sealed record RemoveStaffFromFacultyCommand(
        StaffId StaffId,
        FacultyId FacultyId)
    : ICommand;