namespace Constellation.Application.Students.GetStudentsWithAbsenceSettings;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentsWithAbsenceSettingsQuery
    : IQuery<List<StudentAbsenceSettingsResponse>>;
