﻿namespace Constellation.Application.Domains.Training.Queries.GenerateOverallReport;

using Core.ValueObjects;
using System.Collections.Generic;

public sealed record StaffStatus(
    string StaffId,
    Name Name,
    string SchoolCode,
    string School,
    string[] Faculties,
    List<ModuleStatus> Modules);