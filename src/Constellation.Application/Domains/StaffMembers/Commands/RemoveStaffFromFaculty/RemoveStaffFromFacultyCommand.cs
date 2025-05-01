namespace Constellation.Application.Domains.StaffMembers.Commands.RemoveStaffFromFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;

public sealed record RemoveStaffFromFacultyCommand(
        string StaffId,
        FacultyId FacultyId)
    : ICommand;