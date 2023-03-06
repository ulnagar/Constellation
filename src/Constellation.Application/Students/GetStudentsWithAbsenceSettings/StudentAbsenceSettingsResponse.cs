using Constellation.Core.Enums;
using System;

namespace Constellation.Application.Students.GetStudentsWithAbsenceSettings;

public sealed record StudentAbsenceSettingsResponse(
    string SRN,
    string Name,
    string Gender,
    Grade Grade,
    string School,
    bool AbsenceEnabled,
    DateOnly? AbsenceEnabledFrom);