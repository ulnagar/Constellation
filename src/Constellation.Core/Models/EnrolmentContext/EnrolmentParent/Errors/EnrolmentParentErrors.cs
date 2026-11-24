namespace Constellation.Core.Models.EnrolmentContext.EnrolmentParent.Errors;

using Identifiers;
using Shared;
using System;
using ValueObjects;

public static class EnrolmentParentErrors
{
    public static readonly Func<EnrolmentParentId, Error> NotFound = id => new(
        "Enrolment.Parent.NotFound",
        $"Could not find a parent with the id '{id}'");

    public static readonly Func<EmailAddress, Error> NotFoundByEmail = email => new(
        "Enrolment.Parent.NotFoundByEmail",
        $"Could not find a parent with the email '{email}'");

    public static readonly Error InvalidId = new(
        "Enrolment.Parent.InvalidId",
        "The provided Parent Id is invalid");

    public static readonly Error ParentAlreadyLinkedToUser = new(
        "Enrolment.Parent.ParentAlreadyLinkedToUser",
        "The parent is already linked to a user and cannot be linked to another user");
}
