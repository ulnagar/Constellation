namespace Constellation.Application.Domains.Students.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences.Enums;
using Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;

public sealed record StudentAbsenceSettingsResponse(
    StudentId StudentId,
    string SRN,
    string Name,
    string Gender,
    Grade Grade,
    string School,
    List<StudentAbsenceSettingsResponse.AbsenceConfigurationResponse> AbsenceSettings,
    bool ActiveWhole,
    bool ActivePartial)
{
    public sealed record AbsenceConfigurationResponse(
        AbsenceType AbsenceType,
        DateOnly StartDate,
        DateOnly EndDate);
}