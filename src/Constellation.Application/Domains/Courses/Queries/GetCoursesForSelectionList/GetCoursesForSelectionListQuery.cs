namespace Constellation.Application.Domains.Courses.Queries.GetCoursesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Courses.Models;
using System.Collections.Generic;

public sealed record GetCoursesForSelectionListQuery()
    : IQuery<List<CourseSelectListItemResponse>>;
