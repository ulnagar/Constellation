namespace Constellation.Application.Courses.GetCoursesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Courses.Models;
using System.Collections.Generic;

public sealed record GetCoursesForSelectionListQuery()
    : IQuery<List<CourseSelectListItemResponse>>;
