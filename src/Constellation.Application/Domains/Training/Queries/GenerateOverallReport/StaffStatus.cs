namespace Constellation.Application.Domains.Training.Queries.GenerateOverallReport;

using Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record StaffStatus(
    StaffId StaffId,
    Name Name,
    SchoolCode SchoolCode,
    string School,
    string[] Faculties,
    List<ModuleStatus> Modules);