namespace Constellation.Application.Reports.GetAcademicReportsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAcademicReportsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<StudentReportResponse>>;