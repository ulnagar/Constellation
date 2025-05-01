namespace Constellation.Application.Domains.StudentReports.Queries.GetAcademicReportsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAcademicReportsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<StudentReportResponse>>;