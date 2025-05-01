namespace Constellation.Application.Domains.StudentReports.Queries.GetStudentReportsForSchool;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentReportsForSchoolQuery(
    string SchoolCode)
    : IQuery<List<SchoolStudentReportResponse>>;