namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffList;

using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;
using Faculties.Queries.GetFaculty;
using System.Collections.Generic;

public sealed record StaffResponse(
    StaffId StaffId,
    Name Name,
    List<FacultyResponse> Faculties,
    string School,
    bool IsDeleted);