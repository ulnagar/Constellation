namespace Constellation.Infrastructure.ExternalServices.Canvas;

using Core.Models.Canvas.Models;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
using System;

public static class CanvasGatewayErrors
{
    public static readonly Func<string, Error> UserNotFound = userId => new(
        "CanvasGateway.User.NotFound",
        $"Could not find a user in Canvas with id {userId}");

    public static readonly Func<string, Error> UserLoginNotFound = userId => new(
        "CanvasGateway.User.LoginNotFound",
        $"Could not find a login for user with id {userId}");

    public static readonly Error FailureResponseCode = new(
        "CanvasGateway.FailureResponseCode",
        "The Canvas server responded with an error");

    public static readonly Error InvalidData = new(
        "CanvasGateway.InvalidData",
        "The Canvas server responded with invalid data");

    public static readonly Error RubricNotIncluded = new(
        "CanvasGateway.Assessment.RubricNotIncluded",
        "The requested Assessment does not include a Rubric");

    public static readonly Func<CanvasSectionCode, Error> InvalidSectionCode = section => new(
        "CanvasGateway.Course.InvalidSectionCode",
        $"Could not find a section in Canvas with id {section}");

    public static readonly Func<string, CanvasCourseCode, Error> UserNotFoundInCourse = (userId, courseId) => new(
        "CanvasGateway.Course.UserNotFoundInCourse",
        $"Could not find user with id {userId} in course {courseId}");
}
