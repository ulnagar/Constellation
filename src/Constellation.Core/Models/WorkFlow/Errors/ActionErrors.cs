using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Constellation.Core.Shared;
using System;

namespace Constellation.Core.Models.WorkFlow.Errors;

public static class ActionErrors
{
    public static readonly Func<ActionId, Error> NotFound = id => new(
        "Case.Action.NotFound",
        $"Could not find an Action with the Id {id}");

    public static readonly Error AssignStaffNull = new(
        "Case.Action.Assign.StaffNull",
        "Cannot assign a Case Action to this Staff Member");

    public static readonly Error AssignStaffDeleted = new(
        "Case.Action.Assign.StaffDeleted",
        "Cannot assign a Case Action to a deleted Staff Member");

    public static readonly Error AddNoteMessageBlank = new(
        "Case.Action.AddNote.MessageBlank",
        "Cannot create an ActionNote with a blank message");

    public static readonly Error AddNoteUserNull = new(
        "Case.Action.AddNote.UserNull",
        "Cannot create an ActionNote without a User");

    public static readonly Error AddRecipientDuplicate = new(
        "Case.Action.AddRecipient.Duplicate",
        $"A recipient with that email address already exists in the list");
    
    public static readonly Error AddAttendeeDuplicate = new(
        "Case.Action.AddAttendee.Duplicate",
        $"An attendee with that name already exists in the list");

    public static Func<string, string, Error> CreateCaseTypeMismatch = (expected, provided) => new(
        "Case.Action.Create.CaseTypeMismatch",
        $"Action requires {expected} Case Type, but provided {provided}");

    public static Func<ActionStatus, Error> UpdateStatusAlreadyClosed = status => new(
        "Case.Action.UpdateStatus.Closed",
        $"Cannot change Status as the Action is already {status}");

    public static Error UpdateStatusStatusNull = new(
        "Case.Action.UpdateStatus.StatusNull",
        "Cannot change Status to blank");

    public static readonly Func<string, string, Error> UpdateTypeMismatch = (expected, provided) => new(
        "Case.Action.Update.TypeMismatch",
        $"Expected {expected}, provided {provided}");
    
    public static readonly Error UpdateEmptySubjectLine = new(
        "Case.Action.Update.EmptySubjectLine",
        "Cannot send email with a blank subject line");

    public static readonly Error UpdateEmptyEmailBody = new(
        "Case.Action.Update.EmptyEmailBody",
        "Cannot send email with a blank body");

    public static readonly Error UpdateIncidentNumberZero = new(
        "Case.Action.Update.IncidentNumberZero",
        "Cannot update Action with zero value Incident Number");

    public static readonly Error UpdateNotRequiredFalse = new(
        "Case.Action.Update.NotRequiredFalse",
        "Cannot update Action with Not Required set to false");
}