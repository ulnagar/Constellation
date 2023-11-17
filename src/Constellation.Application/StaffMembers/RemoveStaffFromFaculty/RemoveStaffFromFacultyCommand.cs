namespace Constellation.Application.StaffMembers.RemoveStaffFromFaculty;

using Abstractions.Messaging;
using Core.Models.Faculty.Identifiers;

public sealed record RemoveStaffFromFacultyCommand(
        string StaffId,
        FacultyId FacultyId)
    : ICommand;