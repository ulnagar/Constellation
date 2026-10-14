namespace Constellation.Core.Models.EnrolmentContext.Application.Errors;

using Core.Enums;
using EnrolmentPeriod.Enums;
using Enums;
using Extensions;
using Shared;
using System;
using ApplicationId = Identifiers.ApplicationId;

public static class EnrolmentApplicationErrors
{
    public static readonly Error InvalidId = new(
        "Enrolment.Application.InvalidId",
        "The provided Id is invalid");

    public static readonly Error NoneFound = new(
        "Enrolment.Application.NoneFound",
        "No matching Enrolment Applications could be found");

    public static readonly Func<ApplicationId, Error> NotFound = id => new(
        "Enrolment.Application.NotFound",
        $"Could not find an Enrolment Application with the Id '{id}'");

    public static readonly Error InvalidEnrolmentPeriod = new(
        "Enrolment.Application.InvalidEnrolmentPeriod",
        "The selected Enrolment Period is invalid");

    public static readonly Func<Program, Grade, Error> InvalidProgramGradeCombination = (program, grade) => new(
        "Enrolment.Application.InvalidProgramGradeCombination",
        $"The combination of the '{program}' program and grade {grade.AsName()} is invalid");

    public static readonly Error MultipleExistingApplications = new(
        "Enrolment.Application.MultipleExistingApplications",
        "There are multiple matching applications already recorded for this student");

    public static readonly Error CannotUpdateArchivedApplication = new(
        "Enrolment.Application.CannotUpdateArchivedApplication",
        "Cannot update an Application that has been marked as Archived");

    public static readonly Error CannotUpdateOfferedApplication = new(
        "Enrolment.Application.CannotUpdateOfferedApplication",
        "Cannot update an Application that has been marked as Offered");

    public static readonly Func<EnrolmentCourse, Error> InvalidCourse = (course) => new(
        "Enrolment.Application.CourseNotSelected",
        $"{course.Name} is not currently selected on this application.");

    public static readonly Error ParentRequired = new(
        "Enrolment.Application.ParentRequired",
        "Applications must include at least one parent");

    public static readonly Func<EnrolmentCourse, Error> CourseMustIncludeLine = course => new(
        "Enrolment.Application.CourseMustIncludeLine",
        $"The selected course {course} must include a timetable line");
}
