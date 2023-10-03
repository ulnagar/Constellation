namespace Constellation.Core.Models.Assignments.Errors;

using Identifiers;
using Shared;
using System;

public sealed class SubmissionErrors
{
    public static readonly Func<AssignmentSubmissionId, Error> NotFound = id => new(
        "Assignments.Submission.NotFound",
        $"Could not find any submission with the id {id}");

    public static readonly Error UploadFailed = new(
        "Assignments.Submission.UploadFailed",
        "Assignment Submission could not be uploaded to Canvas server");

    public static readonly Error AlreadyUploaded = new(
        "Assignments.Submission.AlreadyUploaded",
        "Assignment Submission has already been uploaded to Canvas server");
}