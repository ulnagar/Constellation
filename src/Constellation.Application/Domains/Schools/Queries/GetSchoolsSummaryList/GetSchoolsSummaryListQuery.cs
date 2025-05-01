namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsSummaryList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetSchoolsSummaryListQuery(
    SchoolFilter Filter = SchoolFilter.Active)
    : IQuery<List<SchoolSummaryResponse>>;