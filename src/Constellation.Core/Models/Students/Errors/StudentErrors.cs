namespace Constellation.Core.Models.Students.Errors;

using Enums;
using Shared;
using System;

public static class StudentErrors
{
    public static readonly Func<string, Error> AlreadyExists = id => new(
        "Student.AlreadyExists",
        $"A Student with the Id {id} already exists");

    public static readonly Error InvalidId = new(
        "Student.InvalidId",
        "The provided student id is not valid");

    public static readonly Func<string, Error> NotFound = id => new Error(
        "Student.NotFound",
        $"A student with the Id {id} could not be found");

    public static readonly Func<string, Error> NotFoundForSchool = id => new Error(
        "Student.NotFoundForSchool",
        $"No current students found linked to school with Id {id}");

    public static readonly Func<Grade, Error> NotFoundForGrade = grade => new(
        "Student.NotFoundForGrade",
        $"No current students found linked to grade {grade}");

    public static readonly Error NoneFoundFilter = new(
        "Student.NoneFoundFilter",
        $"No current students found that match the selected filter");


    public static readonly Error FirstNameInvalid = new(
        "Student.FirstNameInvalid",
        "The provided First Name is not valid");

    public static readonly Error LastNameInvalid = new(
        "Student.LastNameInvalid",
        "The provided Last Name is not valid");

    public static readonly Error PortalUsernameInvalid = new(
        "Student.PortalUsernameInvalid",
        "The provided Portal Username is not valid");

    public static readonly Error GenderInvalid = new(
        "Student.GenderInvalid",
        "The provided Gender is not valid");

    public static readonly Error SchoolCodeInvalid = new(
        "Student.SchoolCodeInvalid",
        "The provided School Code is not valid");
}