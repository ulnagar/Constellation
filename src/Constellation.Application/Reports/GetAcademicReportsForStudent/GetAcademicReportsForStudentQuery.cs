namespace Constellation.Application.Reports.GetAcademicReportsForStudent;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAcademicReportsForStudentQuery(
    string StudentId)
    : IQuery<List<StudentReportResponse>>;