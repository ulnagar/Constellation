namespace Constellation.Application.Courses.GetCoursesForStudent;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCoursesForStudentQuery(
    string StudentId)
    : IQuery<List<StudentCourseResponse>>;