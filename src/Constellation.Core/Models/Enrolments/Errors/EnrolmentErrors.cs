namespace Constellation.Core.Models.Enrolments.Errors;

using Identifiers;
using Shared;
using System;

public static class EnrolmentErrors
{
    public static readonly Func<EnrolmentId, Error> NotFound = id => new(
        "Enrolment.NotFound",
        $"Could not find an Enrolment with the Id {id}");
    public static Error AlreadyDeleted => new(
        "Enrolment.Enrolment",
        "This enrolment is already marked deleted");
}
