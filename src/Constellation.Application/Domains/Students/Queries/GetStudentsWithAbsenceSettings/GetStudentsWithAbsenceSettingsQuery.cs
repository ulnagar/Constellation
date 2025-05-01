namespace Constellation.Application.Domains.Students.Queries.GetStudentsWithAbsenceSettings;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentsWithAbsenceSettingsQuery
    : IQuery<List<StudentAbsenceSettingsResponse>>;
