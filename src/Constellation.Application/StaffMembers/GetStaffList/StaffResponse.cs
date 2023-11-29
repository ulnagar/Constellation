namespace Constellation.Application.StaffMembers.GetStaffList;

using Core.ValueObjects;
using Faculties.GetFaculty;
using System.Collections.Generic;

public sealed record StaffResponse(
    string StaffId,
    Name Name,
    List<FacultyResponse> Faculties,
    string School,
    bool IsDeleted);