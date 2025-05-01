namespace Constellation.Application.Domains.Courses.Queries.GetCourseSummaryList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Courses.Models;
using System.Collections.Generic;

public sealed record GetCourseSummaryListQuery()
    : IQuery<List<CourseSummaryResponse>>;
