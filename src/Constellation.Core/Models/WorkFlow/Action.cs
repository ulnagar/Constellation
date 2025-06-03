#nullable enable
using Constellation.Core.Models.Training;

namespace Constellation.Core.Models.WorkFlow;

using Enums;
using Errors;
using Identifiers;
using Offerings;
using Offerings.Identifiers;
using Primitives;
using Shared;
using StaffMembers;
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

    internal Result AssignAction(StaffMember? assignee, string currentUser)
    {
        if (assignee is null)
            return Result.Failure(ActionErrors.AssignStaffNull);

        if (assignee.IsDeleted)
            return Result.Failure(ActionErrors.AssignStaffDeleted);

        if (!string.IsNullOrWhiteSpace(AssignedTo))
        {
            Result noteAttempt = AddNote($"Assignee changed from {AssignedTo} to {assignee.Name.DisplayName}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }
        else
        {
            Result noteAttempt = AddNote($"Assignee set to {assignee.Name.DisplayName}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }

        AssignedToId = assignee.Id.ToString();
        AssignedTo = assignee.Name.DisplayName;
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
            return Result.Failure(ActionErrors.AddNoteMessageBlank);

        if (string.IsNullOrWhiteSpace(currentUser))
            return Result.Failure(ActionErrors.AddNoteUserNull);
        
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
            return Result.Failure(ActionErrors.AddRecipientDuplicate);

        _recipients.Add(recipient);

        return Result.Success();
    }

    public static Result<SendEmailAction> Create(
        CaseId caseId,
        StaffMember assignee,
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
    /// <returns>Result</returns>
    public Result Update(
        IReadOnlyList<EmailRecipient> recipients,
        EmailRecipient sender,
        string subject,
        string body,
        bool hasAttachments,
        DateTime sentAt)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure(ActionErrors.UpdateEmptySubjectLine);

        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure(ActionErrors.UpdateEmptyEmailBody);

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
        StaffMember assignee,
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
            return Result.Failure(ActionErrors.UpdateIncidentNumberZero);

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
            return Result.Failure(ActionErrors.AddAttendeeDuplicate);

        _attendees.Add(attendee);

        return Result.Success();
    }
    
    public static Result<ParentInterviewAction> Create(
        CaseId caseId,
        StaffMember assignee,
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
        IReadOnlyList<InterviewAttendee> attendees,
        DateTime dateOccurred,
        int incidentNumber,
        string currentUser)
    {
        if (incidentNumber == 0)
            return Result.Failure(ActionErrors.UpdateIncidentNumberZero);

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
            return Result.Failure(ActionErrors.UpdateIncidentNumberZero);

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
            return Result.Failure(ActionErrors.UpdateNotRequiredFalse);

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
        StaffMember assignee,
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
        StaffMember assignee,
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
        StaffMember assignee,
        string details)
    {
        CaseDetailUpdateAction action = new()
        {
            CaseId = caseId,
            ParentActionId = parentId,
            Details = details
        };

        Result assignment = action.AssignAction(assignee, assignee.Name.DisplayName);

        if (assignment.IsFailure)
            return Result.Failure<CaseDetailUpdateAction>(assignment.Error);

        Result status = action.UpdateStatus(ActionStatus.Completed, assignee.Name.DisplayName);

        if (status.IsFailure)
            return Result.Failure<CaseDetailUpdateAction>(status.Error);

        return action;
    }

    public override string ToString() =>
        "Case Detail Update entry";

    public override string AsStatus() =>
        $"Case detail updated by {AssignedTo}";
}

public sealed class SentralIncidentStatusAction : Action
{
    public override string Description => $"Record of the outcome of a Sentral Incident identified in this case";

    public override string ToString() => 
        $"Update Sentral Incident status and record details";

    public override string AsStatus() =>
        Status switch
        {
            { } value when value.Equals(ActionStatus.Open) =>
                $"Task assigned to: {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) && MarkedResolved =>
                $"Sentral incident marked Resolved by {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) && MarkedNotCompleted =>
                $"Sentral incident marked Not Completed and reissued as Incident {IncidentNumber} completed by {AssignedTo}",
            _ => $"Unknown Status"
        };

    public int IncidentNumber { get; private set; }
    public bool MarkedResolved { get; private set; }
    public bool MarkedNotCompleted { get; private set; }

    private SentralIncidentStatusAction() { }

    public static Result<SentralIncidentStatusAction> Create(
        CaseId caseId,
        StaffMember assignee,
        string currentUser)
    {
        SentralIncidentStatusAction action = new()
        {
            CaseId = caseId
        };

        Result assignment = action.AssignAction(assignee, currentUser);

        if (assignment.IsFailure)
            return Result.Failure<SentralIncidentStatusAction>(assignment.Error);

        return action;
    }

    public Result Update(int followUpIncidentId, string currentUser)
    {
        if (followUpIncidentId == 0)
            return Result.Failure(ActionErrors.UpdateIncidentNumberZero);

        if (IncidentNumber != 0)
        {
            Result noteAttempt = AddNote($"MarkedNotCompleted set to true with previous Incident Number changed from {IncidentNumber} to {followUpIncidentId}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }
        else
        {
            Result noteAttempt = AddNote($"MarkedNotCompleted set to true with Incident Number set to {followUpIncidentId}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }

        MarkedResolved = false;
        MarkedNotCompleted = true;
        IncidentNumber = followUpIncidentId;

        return Result.Success();
    }

    public Result Update(string currentUser)
    {
        Result noteAttempt = AddNote($"MarkedResolved set to true", currentUser);

        if (noteAttempt.IsFailure)
            return noteAttempt;

        MarkedResolved = true;
        MarkedNotCompleted = false;
        IncidentNumber = 0;

        return Result.Success();
    }
}

public sealed class UploadTrainingCertificateAction : Action
{
    public override string Description => $"Complete Training Module and Upload Certificate";
    public override string ToString() =>
        $"Complete Training Module and Upload Certificate";

    public override string AsStatus() =>
        Status switch
        {
            { } value when value.Equals(ActionStatus.Open) =>
                $"Task assigned to: {AssignedTo}",
            { } value when value.Equals(ActionStatus.Completed) =>
                $"Training certificate uploaded",
            _ => $"Unknown Status"
        };

    public string ModuleName { get; private set; } = string.Empty;

    private UploadTrainingCertificateAction() { }

    public static Result<UploadTrainingCertificateAction> Create(
        CaseId caseId,
        TrainingModule module,
        StaffMember assignee,
        string currentUser)
    {
        UploadTrainingCertificateAction action = new()
        {
            CaseId = caseId,
            ModuleName = module.Name
        };

        Result assignment = action.AssignAction(assignee, currentUser);

        if (assignment.IsFailure)
            return Result.Failure<UploadTrainingCertificateAction>(assignment.Error);

        return action;
    }

    public Result Update(string currentUser)
    {
        Result noteAttempt = AddNote($"Training Certification uploaded", currentUser);

        if (noteAttempt.IsFailure)
            return noteAttempt;

        return Result.Success();
    }
}