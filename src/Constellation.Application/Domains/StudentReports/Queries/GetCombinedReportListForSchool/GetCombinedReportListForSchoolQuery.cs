namespace Constellation.Application.Domains.StudentReports.Queries.GetCombinedReportListForSchool;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using GetCombinedReportListForStudent;
using System.Collections.Generic;

public sealed record GetCombinedReportListForSchoolQuery(
    SchoolCode SchoolCode)
    : IQuery<List<SchoolReportResponse>>;