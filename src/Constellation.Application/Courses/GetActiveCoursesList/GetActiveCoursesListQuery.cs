namespace Constellation.Application.Courses.GetActiveCoursesList;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetActiveCoursesListQuery()
    : IQuery<List<CourseSelectListItemResponse>>;