namespace Constellation.Application.Courses.GetCoursesForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetCoursesForStudentQuery(
    StudentId StudentId)
    : IQuery<List<StudentCourseResponse>>;