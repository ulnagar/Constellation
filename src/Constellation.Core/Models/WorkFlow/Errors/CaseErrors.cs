namespace Constellation.Core.Models.WorkFlow.Errors;

using Constellation.Core.Models.Students.Identifiers;
using Identifiers;
using Shared;
using System;

public static class CaseErrors
{
    public static readonly Func<CaseId, Error> NotFound = id => new(
        "Case.NotFound",
        $"Could not find a Case with the Id {id}");

    public static readonly Func<StudentId, Error> NotFoundForStudent = id => new(
        "Case.NotFoundForStudent",
        $"Could not find a Case for Student with Id {id}");

    public static readonly Func<DateOnly, Error> DueDateInPast = date => new(
        "Case.DueDateInPast",
        $"Could not set the Due Date for the Case to {date} as this has already passed");

    public static readonly Func<string, string, Error> AttachDetailsDetailMismatch = (caseType, expected) => new(
        "Case.AttachDetails.DetailMismatch",
        $"Require details of type {expected} for Case Type {caseType}");

    public static readonly Error AttachDetailsUnknownDetails = new(
        "Case.AttachDetails.UnknownDetails",
        "Unable to determine correct details type to attach");

    public static readonly Error UpdateStatusCompletedWithOutstandingActions = new(
        "Case.UpdateStatus.CompletedWithOutstandingActions",
        "Cannot mark a Case completed with pending actions");
}