namespace Constellation.Application.Domains.StudentReports.Queries.GetExternalReportList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetExternalReportListQuery()
    : IQuery<List<ExternalReportResponse>>;
