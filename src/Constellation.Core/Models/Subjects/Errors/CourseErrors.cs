namespace Constellation.Core.Models.Subjects.Errors;

using Identifiers;
using Offerings.Identifiers;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CourseErrors
{
    public static readonly Error CodeEmpty = new(
        "Subjects.Course.CodeEmpty",
        "A code must be supplied to create a Course");

    public static readonly Error CodeLengthInvalid = new(
        "Subjects.Course.CodeLengthInvalid",
        "The length of the code supplied is invalid");

    public static readonly Func<CourseId, Error> NoOfferings = id => new(
        "Subjects.Course.NoOfferings",
        $"Could not find any offerings related to course with id {id}");

    public static readonly Func<CourseId, Error> NotFound = id => new(
        "Subjects.Course.NotFound",
        $"Could not find a course with the id {id}");

    public static readonly Func<OfferingId, Error> NotFoundByOfferingId = id => new(
        "Subjects.Course.NotFoundByOfferingId",
        $"Could not fina a course linked with offering with id {id}");

    public static readonly Error NoneFound = new(
        "Subjects.Course.NoneFound",
        "No Courses could be found");

    public static readonly Error IsNull = new(
        "Subjects.Course.IsNull",
        "A Course is required, but none supplied");
}
