namespace Constellation.Core.Models.Tutorials.Errors;

using Identifiers;
using Shared;
using System;

public sealed class TutorialRequestErrors
{
    public static Func<RequestId, Error> NotFound = id => new(
        "Tutorial.Request.NotFound",
        $"Could not find a Tutorial Request with the Id {id}");

    public static Error AlreadyReviewed = new(
        "Tutorial.Request.AlreadyReviewed",
        "The Tutorial Request has already been reviewed");

    public static Error MustIncludeNote = new(
        "Tutorial.Request.MustIncludeNote",
        "A review of a Tutorial Request must include a note");

    public static Error InvalidStatus = new(
        "Tutorial.Request.InvalidStatus",
        "Cannot update Request Status due to invalid process path");

    public static Error InvalidStartDate = new(
        "Tutorial.Request.InvalidStartDate",
        "A Start Date must be provided when scheduling a Tutorial Request");

    public static Error PlanAlreadySubmitted = new(
        "Tutorial.Request.PlanAlreadySubmitted",
        "A Plan has already been submitted for this Tutorial Request");

    public static Func<RequestId, Error> PlanNotFound = id => new(
        "Tutorial.Request.PlanNotFound",
        $"Could not find a Plan associated with Tutorial Request with the Id {id}");
}