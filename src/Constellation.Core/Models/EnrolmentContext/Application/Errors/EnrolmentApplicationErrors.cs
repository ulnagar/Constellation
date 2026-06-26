namespace Constellation.Core.Models.EnrolmentContext.Application.Errors;

using Shared;
using System;
using ApplicationId = Identifiers.ApplicationId;

public static class EnrolmentApplicationErrors
{
    public static readonly Func<ApplicationId, Error> NotFound = id => new(
        "Enrolment.Application.NotFound",
        $"Could not find an Enrolment Application with the Id '{id}'");

}
