namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAbsencesForStudentQuery(
    StudentId StudentId)
    : IQuery<List<AbsenceForStudentResponse>>;