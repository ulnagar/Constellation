namespace Constellation.Application.Students.GetStudentsFromCourseAsDictionary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetStudentsFromCourseAsDictionaryQuery(
    CourseId CourseId)
    : IQuery<Dictionary<StudentId, string>>;
