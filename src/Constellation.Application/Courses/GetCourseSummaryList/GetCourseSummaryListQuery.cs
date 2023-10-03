namespace Constellation.Application.Courses.GetCourseSummaryList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Courses.Models;
using System.Collections.Generic;

public sealed record GetCourseSummaryListQuery()
    : IQuery<List<CourseSummaryResponse>>;
