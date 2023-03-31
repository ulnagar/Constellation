namespace Constellation.Application.Students.GetStudentsFromCourseAsDictionary;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentsFromCourseAsDictionaryQuery(
    int CourseId)
    : IQuery<Dictionary<string, string>>;
