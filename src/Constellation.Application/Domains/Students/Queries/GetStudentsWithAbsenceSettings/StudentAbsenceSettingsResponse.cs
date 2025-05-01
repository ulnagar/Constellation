using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Domains.Students.Queries.GetStudentsWithAbsenceSettings;

public sealed record StudentAbsenceSettingsResponse(
    string SRN,
    string Name,
    string Gender,
    Grade Grade,
    string School,
    List<StudentAbsenceSettingsResponse.AbsenceConfigurationResponse> AbsenceSettings)
{
    public sealed record AbsenceConfigurationResponse(
        AbsenceType AbsenceType,
        DateOnly StartDate,
        DateOnly EndDate);
}