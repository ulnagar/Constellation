namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffList;

using Core.ValueObjects;
using Faculties.Queries.GetFaculty;
using System.Collections.Generic;

public sealed record StaffResponse(
    string StaffId,
    Name Name,
    List<FacultyResponse> Faculties,
    string School,
    bool IsDeleted);