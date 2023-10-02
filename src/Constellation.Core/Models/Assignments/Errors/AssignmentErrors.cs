namespace Constellation.Core.Models.Assignments.Errors;

using Identifiers;
using Shared;
using Subjects.Identifiers;
using System;

public sealed class AssignmentErrors
{
    public static readonly Func<AssignmentId, Error> NotFound = id => new(
        "Assignments.Assignment.NotFound",
        $"Could not find an assignment with the Id {id}");

    public static readonly Func<CourseId, Error> NotFoundByCourse = id => new(
        "Assignments.Assignment.NotFoundByCourse",
        $"Could not find any assignments linked to the course with id {id}");
}