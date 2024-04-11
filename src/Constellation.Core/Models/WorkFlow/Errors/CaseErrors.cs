namespace Constellation.Core.Models.WorkFlow.Errors;

using Enums;
using Identifiers;
using Shared;
using System;

public static class CaseErrors
{
    public static class Case
    {
        public static readonly Func<CaseId, Error> NotFound = id => new(
            "Case.NotFound",
            $"Could not find a Case with the Id {id}");

        public static class AttachDetails
        {
            public static readonly Func<string, string, Error> DetailMismatch = (caseType, expected) => new(
                "Case.AttachDetails.DetailMismatch",
                $"Require details of type {expected} for Case Type {caseType}");

            public static readonly Error UnknownDetails = new(
                "Case.AttachDetails.UnknownDetails",
                "Unable to determine correct details type to attach");
        }

        public static class UpdateStatus
        {
            public static readonly Error CompletedWithOutstandingActions = new(
                "Case.UpdateStatus.CompletedWithOutstandingActions",
                "Cannot mark a Case completed with pending actions");
        }
    }

    public static class CaseDetail
    {
        public static class Create
        {
            public static readonly Error StudentNull = new(
                "Case.Detail.Create.StudentNull",
                "The student cannot be empty or null");

            public static readonly Error AttendanceValueNull = new(
                "Case.Detail.Create.AttendanceValueNull",
                "The Attendance Value cannot be empty or null");

            public static readonly Error StudentMismatch = new(
                "Case.Detail.Create.StudentMismatch",
                "The Attendance Value is not linked to the Student");
        }
    }

    public static class Action
    {
        public static readonly Func<ActionId, Error> NotFound = id => new(
            "Case.Action.NotFound",
            $"Could not find an Action with the Id {id}");

        public static class Assign
        {
            public static readonly Error StaffNull = new(
                "Case.Action.Assign.StaffNull",
                "Cannot assign a Case Action to this Staff Member");

            public static readonly Error StaffDeleted = new(
                "Case.Action.Assign.StaffDeleted",
                "Cannot assign a Case Action to a deleted Staff Member");
        }

        public static class AddNote
        {
            public static readonly Error MessageBlank = new(
                "Case.Action.AddNote.MessageBlank",
                "Cannot create an ActionNote with a blank message");

            public static readonly Error UserNull = new(
                "Case.Action.AddNote.UserNull",
                "Cannot create an ActionNote without a User");
        }

        public static class AddRecipient
        {
            public static readonly Error Duplicate = new(
                "Case.Action.AddRecipient.Duplicate",
                $"A recipient with that email address already exists in the list");
        }

        public static class Create
        {
            public static Func<string, string, Error> CaseTypeMismatch = (expected, provided) => new(
                "Case.Action.Create.CaseTypeMismatch",
                $"Action requires {expected} Case Type, but provided {provided}");
        }

        public static class UpdateStatus
        {
            public static Func<ActionStatus, Error> AlreadyClosed = status => new(
                "Case.Action.UpdateStatus.Closed",
                $"Cannot change Status as the Action is already {status}");

            public static Error StatusNull = new(
                "Case.Action.UpdateStatus.StatusNull",
                "Cannot change Status to blank");
        }

        public static class Update
        {
            public static readonly Func<string, string, Error> TypeMismatch = (expected, provided) => new(
                "Case.Action.Update.TypeMismatch",
                $"Expected {expected}, provided {provided}");
            

            public static readonly Error EmptySubjectLine = new(
                "Case.Action.Update.EmptySubjectLine",
                "Cannot send email with a blank subject line");

            public static readonly Error EmptyEmailBody = new(
                "Case.Action.Update.EmptyEmailBody",
                "Cannot send email with a blank body");

            public static readonly Error IncidentNumberZero = new(
                "Case.Action.Update.IncidentNumberZero",
                "Cannot update Action with zero value Incident Number");

            public static readonly Error NotRequiredFalse = new(
                "Case.Action.Update.NotRequiredFalse",
                "Cannot update Action with Not Required set to false");
        }
    }
}
