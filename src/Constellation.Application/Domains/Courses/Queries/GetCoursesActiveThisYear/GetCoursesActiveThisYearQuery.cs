namespace Constellation.Application.Domains.Courses.Queries.GetCoursesActiveThisYear;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetCoursesActiveThisYearQuery()
    : IQuery<List<CourseSelectListItemResponse>>;