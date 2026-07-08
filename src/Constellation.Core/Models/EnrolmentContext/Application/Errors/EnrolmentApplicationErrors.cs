namespace Constellation.Core.Models.EnrolmentContext.Application.Errors;

using Enums;
using Extensions;
using Offer.Enums;
using Shared;
using System;
using ApplicationId = Identifiers.ApplicationId;

public static class EnrolmentApplicationErrors
{
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
}
