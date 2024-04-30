#nullable enable
namespace Constellation.Core.Models.WorkFlow;

using Enums;
using Errors;
using Identifiers;
using Offerings;
using Offerings.Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public abstract class Action : IAuditableEntity
{
    private readonly List<ActionNote> _notes = new();

    protected Action() { }

    public ActionId Id { get; private protected set; } = new();
    public CaseId? CaseId { get; private protected set; }
    public ActionId? ParentActionId { get; private protected set; }
    public ActionStatus Status { get; private protected set; } = ActionStatus.Open;
    public IReadOnlyList<ActionNote> Notes => _notes.ToList();
    
    // string StaffId column
    public string AssignedToId { get; protected set; } = string.Empty;
    // string Staff Name column
    public string AssignedTo { get; protected set; } = string.Empty;
    public DateTime AssignedAt { get; protected set; } = DateTime.MinValue;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; } = DateTime.MinValue;
    public bool IsDeleted { get; internal set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; } = DateTime.MinValue;


    public abstract string Description { get; }
    public abstract override string ToString();
    public abstract string AsStatus();

    internal Result AssignAction(Staff? assignee, string currentUser)
    {
        if (assignee is null)
            return Result.Failure(CaseErrors.Action.Assign.StaffNull);

        if (assignee.IsDeleted)
            return Result.Failure(CaseErrors.Action.Assign.StaffDeleted);

        if (!string.IsNullOrWhiteSpace(AssignedTo))
        {
            Result noteAttempt = AddNote($"Assignee changed from {AssignedTo} to {assignee.GetName()!.DisplayName}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }
        else
        {
            Result noteAttempt = AddNote($"Assignee set to {assignee.GetName()!.DisplayName}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }

        AssignedToId = assignee.StaffId;
        AssignedTo = assignee.GetName()!.DisplayName;
        AssignedAt = DateTime.Now;

        return Result.Success();
    }

    internal Result UpdateStatus(
        ActionStatus newStatus,
        string currentUser)
    {
        Result noteAttempt = AddNote($"Status changed from {Status} to {newStatus}", currentUser);

        if (noteAttempt.IsFailure)
            return noteAttempt;

        Status = newStatus;

        return Result.Success();
    }

    internal Result AddNote(string message, string currentUser)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure(CaseErrors.Action.AddNote.MessageBlank);

        if (string.IsNullOrWhiteSpace(currentUser))
            return Result.Failure(CaseErrors.Action.AddNote.UserNull);
        
        ActionNote note = ActionNote.Create(
            message,
            currentUser,
            DateTime.Now);

        _notes.Add(note);

        return Result.Success();
    }
}

public sealed class SendEmailAction : Action
{
    // Option to send email through Constellation (template or free-form)
    // Option to upload copy of email sent separately for records

    private readonly List<EmailRecipient> _recipients = new();

    private SendEmailAction()
        : base() { }

    public override string Description => "Send an email";

    public IReadOnlyList<EmailRecipient> Recipients => _recipients.AsReadOnly();

    public EmailRecipient? Sender { get; private set; }

    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;

    public DateTime SentAt { get; private set; } = DateTime.MinValue;

    // Include ability to send attachments with the email
    // Should be stored using the AttachmentService and linked using the ActionId for this action
    // Property here symbolises whether there are attachments to search for
    public bool HasAttachments { get; private set; }

    private Result AddRecipient(EmailRecipient recipient)
    {
        if (_recipients.Any(entry => entry.Email == recipient.Email))
            return Result.Failure(CaseErrors.Action.AddRecipient.Duplicate);

        _recipients.Add(recipient);

        return Result.Success();
    }

    public static Result<SendEmailAction> Create(
        CaseId caseId,
        Staff assignee,
        string currentUser)
    {
        SendEmailAction action = new()
        {
            CaseId = caseId
        };

        Result assignment = action.AssignAction(assignee, currentUser);

        if (assignment.IsFailure)
            return Result.Failure<SendEmailAction>(assignment.Error);

        return action;
    }

    /// <summary>
    /// Create a record of an email that is about to be sent
    /// </summary>
    /// <param name="recipients">List of EmailRecipient recipients of the email</param>
    /// <param name="sender">EmailRecipient symbolising the sender of the email</param>
    /// <param name="subject">Subject line of the email</param>
    /// <param name="body">Body of the email as raw HTML string</param>
    /// <param name="hasAttachments">Has the email been sent with any attached files?</param>
    /// <param name="sentAt">At what DateTime was the email queued for sending?</param>
    /// <param name="currentUser">Which user initiated this action?</param>
    /// <returns>Result</returns>
    public Result Update(
        List<EmailRecipient> recipients,
        EmailRecipient sender,
        string subject,
        string body,
        bool hasAttachments,
        DateTime sentAt,
        string currentUser)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure(CaseErrors.Action.Update.EmptySubjectLine);

        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure(CaseErrors.Action.Update.EmptyEmailBody);

        foreach (EmailRecipient recipient in recipients)
        {
            Result recipientAction = AddRecipient(recipient);

            if (recipientAction.IsFailure)
                return recipientAction;
        }

        Sender = sender;
        Subject = subject;
        Body = body;
        HasAttachments = hasAttachments;
        SentAt = sentAt;

        return Result.Success();
    }

    public override string ToString() =>
        $"Send email";

    public override string AsStatus() =>
        Status switch
        {
            { } value when value.Equals(ActionStatus.Open) => 
                $"Task assigned to: {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) =>
                $"Email sent to {Recipients.Count} recipients with subject {Subject} by {AssignedTo}",
            _ => $"Unknown Status"
        };
}

public sealed class PhoneParentAction : Action
{
    public override string Description => $"Record of a phone call with a parent or guardian";

    // Option to contact parent via phone to discuss issues
    public string? ParentName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTime DateOccurred { get; private set; }
    public int IncidentNumber { get; private set; }

    public static Result<PhoneParentAction> Create(
        CaseId caseId,
        Staff assignee,
        string currentUser)
    {
        PhoneParentAction action = new()
        {
            CaseId = caseId
        };

        Result assignment = action.AssignAction(assignee, currentUser);

        if (assignment.IsFailure)
            return Result.Failure<PhoneParentAction>(assignment.Error);

        return action;
    }

    public Result Update(
        string parentName,
        string parentNumber,
        DateTime dateOccurred,
        int incidentNumber, 
        string currentUser)
    {
        if (incidentNumber == 0)
            return Result.Failure(CaseErrors.Action.Update.IncidentNumberZero);

        Result noteAttempt = AddNote(
            $"Details updated: ParentName = {parentName}, PhoneNumber = {parentNumber}, DateOccurred = {dateOccurred}, IncidentNumber = {incidentNumber}",
            currentUser);
        
        if (noteAttempt.IsFailure)
            return noteAttempt;
       
        ParentName = parentName;
        PhoneNumber = parentNumber;
        DateOccurred = dateOccurred;
        IncidentNumber = incidentNumber;
        
        return Result.Success();
    }

    public override string ToString() =>
        $"Contact Parent via phone";

    public override string AsStatus() =>
        Status switch
        {
            { } value when value.Equals(ActionStatus.Open) =>
                $"Task assigned to: {AssignedTo}",
            _ => $"Unknown Status"
        };
}

public sealed class ParentInterviewAction : Action
{
    private readonly List<InterviewAttendee> _attendees = new();

    public override string Description => $"Record of an interview with a parent or guardian";

    public IReadOnlyList<InterviewAttendee> Attendees => _attendees.AsReadOnly();

    // Option to schedule and record interview with parent
    public DateTime DateOccurred { get; private set; }
    public int IncidentNumber { get; private set; }

    private Result AddAttendee(InterviewAttendee attendee)
    {
        if (_attendees.Any(entry => entry.Name == attendee.Name))
            return Result.Failure(CaseErrors.Action.AddAttendee.Duplicate);

        _attendees.Add(attendee);

        return Result.Success();
    }
    
    public static Result<ParentInterviewAction> Create(
        CaseId caseId,
        Staff assignee,
        string currentUser)
    {
        ParentInterviewAction action = new()
        {
            CaseId = caseId
        };

        Result assignment = action.AssignAction(assignee, currentUser);

        if (assignment.IsFailure)
            return Result.Failure<ParentInterviewAction>(assignment.Error);

        return action;
    }

    public Result Update(
        List<InterviewAttendee> attendees,
        DateTime dateOccurred,
        int incidentNumber,
        string currentUser)
    {
        if (incidentNumber == 0)
            return Result.Failure(CaseErrors.Action.Update.IncidentNumberZero);

        foreach (InterviewAttendee attendee in attendees)
        {
            Result attendeeAction = AddAttendee(attendee);

            if (attendeeAction.IsFailure)
                return attendeeAction;
        }

        Result noteAttempt = AddNote(
            $"Details updated: DateOccurred = {dateOccurred}, IncidentNumber = {incidentNumber}",
            currentUser);

        if (noteAttempt.IsFailure)
            return noteAttempt;

        DateOccurred = dateOccurred;
        IncidentNumber = incidentNumber;

        return Result.Success();
    }

    public override string ToString() =>
        $"Arrange interview with Parent";

    public override string AsStatus() =>
        Status switch
        {
            { } value when value.Equals(ActionStatus.Open) =>
                $"Task assigned to: {AssignedTo}",
            _ => $"Unknown Status"
        };
}

public sealed class CreateSentralEntryAction : Action
{
    public override string Description => $"Record of the reference to a Sentral Incident that has been created relating to this case";

    // Option to record Sentral Incident creation (Incident Id only)

    private CreateSentralEntryAction()
        : base() { }

    public OfferingId? OfferingId { get; private set; }
    public string OfferingName { get; private set; } = string.Empty;

    public bool NotRequired { get; private set; }
    public int IncidentNumber { get; private set; }

    public Result Update(int incidentNumber, string currentUser)
    {
        if (incidentNumber == 0)
            return Result.Failure(CaseErrors.Action.Update.IncidentNumberZero);

        if (IncidentNumber != 0)
        {
            Result noteAttempt = AddNote($"Incident Number changed from {IncidentNumber} to {incidentNumber}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }
        else
        {
            Result noteAttempt = AddNote($"Incident Number set to {incidentNumber}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }
        
        IncidentNumber = incidentNumber;

        return Result.Success();
    }

    public Result Update(bool notRequired, string currentUser)
    {
        if (notRequired == false)
            return Result.Failure(CaseErrors.Action.Update.NotRequiredFalse);

        if (IncidentNumber != 0)
        {
            Result noteAttempt = AddNote($"Not Required set to true with previous Incident Number value of {IncidentNumber}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;

            IncidentNumber = 0;
        }
        else
        {
            Result noteAttempt = AddNote($"Not Required set to true", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }

        NotRequired = true;

        return Result.Success();
    }

    public static Result<CreateSentralEntryAction> Create(
        CaseId caseId,
        Staff assignee,
        Offering offering,
        string currentUser)
    {
        CreateSentralEntryAction action = new()
        {
            CaseId = caseId,
            OfferingId = offering.Id,
            OfferingName = offering.Name
        };

        Result assignment = action.AssignAction(assignee, currentUser);

        if (assignment.IsFailure)
            return Result.Failure<CreateSentralEntryAction>(assignment.Error);

        return action;
    }

    public override string ToString() =>
        $"Create Sentral entry and record Incident Number: {OfferingName}";

    public override string AsStatus() =>
        Status switch
        {
            { } value when value.Equals(ActionStatus.Open) =>
                $"Task assigned to: {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) && NotRequired =>
                $"Sentral entry determined Not Required by {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) && !NotRequired =>
                $"Sentral entry {IncidentNumber} completed by {AssignedTo}",
            _ => $"Unknown Status"
        };
}

public sealed class ConfirmSentralEntryAction : Action
{
    // Option to have user confirm Sentral Incident recorded in the CreateSentralEntryAction was actually created
    public override string Description => $"Record of confirmation that the reference to a Sentral Incident above has been created";

    private ConfirmSentralEntryAction() 
        : base() { }

    public bool Confirmed { get; private set; }

    public Result Update(bool confirmed, string currentUser)
    {
        Result noteAttempt = AddNote($"Confirmed set to {confirmed}", currentUser);

        if (noteAttempt.IsFailure)
            return noteAttempt;

        Confirmed = confirmed;

        return Result.Success();
    }

    public static Result<ConfirmSentralEntryAction> Create(
        ActionId parentId,
        CaseId caseId,
        Staff assignee,
        string currentUser)
    {
        ConfirmSentralEntryAction action = new()
        {
            CaseId = caseId, 
            ParentActionId = parentId
        };

        Result assignment = action.AssignAction(assignee, currentUser);

        if (assignment.IsFailure)
            return Result.Failure<ConfirmSentralEntryAction>(assignment.Error);

        return action;
    }

    public override string ToString() =>
        $"Confirm Sentral entry";

    public override string AsStatus() =>
        Status switch
        {
            { } value when value.Equals(ActionStatus.Open) =>
                $"Task assigned to: {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) && Confirmed =>
                $"Sentral entry confirmed by {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) && !Confirmed =>
                $"Sentral entry rejected by {AssignedTo}",
            _ => $"Unknown Status"
        };
}

public sealed class CaseDetailUpdateAction : Action
{
    private CaseDetailUpdateAction() { }

    public override string Description => "Record of further information or actions taken in relation to this case";

    public string Details { get; private set; } = string.Empty;

    public static Result<CaseDetailUpdateAction> Create(
        ActionId? parentId,
        CaseId caseId,
        Staff assignee,
        string details)
    {
        CaseDetailUpdateAction action = new()
        {
            CaseId = caseId,
            ParentActionId = parentId,
            Details = details
        };

        Result assignment = action.AssignAction(assignee, assignee.DisplayName);

        if (assignment.IsFailure)
            return Result.Failure<CaseDetailUpdateAction>(assignment.Error);

        Result status = action.UpdateStatus(ActionStatus.Completed, assignee.DisplayName);

        if (status.IsFailure)
            return Result.Failure<CaseDetailUpdateAction>(status.Error);

        return action;
    }

    public override string ToString() =>
        "Case Detail Update entry";

    public override string AsStatus() =>
        $"Case detail updated by {AssignedTo}";
}

