namespace Constellation.Core.Models.EnrolmentContext.EnrolmentParent.Errors;

using Identifiers;
using Shared;
using System;

public static class EnrolmentParentErrors
{
    public static readonly Func<EnrolmentParentId, Error> NotFound = id => new(
        "Enrolment.Parent.NotFound",
        $"Could not find a parent with the id '{id}'");

    public static readonly Error InvalidId = new(
        "Enrolment.Parent.InvalidId",
        "The provided Parent Id is invalid");
}
