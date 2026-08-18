namespace Constellation.Application.Domains.Students.Queries.GetStudentsWithAbsenceSettings;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Students.Models;
using System.Collections.Generic;

public sealed record GetStudentsWithAbsenceSettingsQuery
    : IQuery<List<StudentAbsenceSettingsResponse>>;
