namespace Constellation.Application.Courses.GetCoursesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCoursesForSelectionListQuery()
    : IQuery<List<CourseSelectListItemResponse>>;
