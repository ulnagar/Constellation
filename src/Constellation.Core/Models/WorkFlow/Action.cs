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

    internal Result AssignAction(Staff? assignee, string currentUser)
    {
        if (assignee is null)
            return Result.Failure(CaseErrors.Action.Assign.StaffNull);

        if (assignee.IsDeleted)
            return Result.Failure(CaseErrors.Action.Assign.StaffDeleted);

        if (!string.IsNullOrWhiteSpace(AssignedTo))
        {
            Result noteAttempt = AddNote($"Assignee changed from {AssignedTo} to {assignee.GetName()!.DisplayName} by {currentUser}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }
        else
        {
            Result noteAttempt = AddNote($"Assignee set to {assignee.GetName()!.DisplayName} by {currentUser}", currentUser);

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
        Result noteAttempt = AddNote($"Status changed from {Status} to {newStatus} by {currentUser}", currentUser);

        if (noteAttempt.IsFailure)
            return noteAttempt;

        Status = newStatus;

        return Result.Success();
    }

    protected Result AddNote(string message, string currentUser)
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
    /// <returns>Result</returns>
    public Result Update(
        List<EmailRecipient> recipients,
        EmailRecipient sender,
        string subject,
        string body,
        bool hasAttachments,
        DateTime sentAt)
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
}

public sealed class PhoneParentAction : Action
{
    // Option to contact parent via phone to discuss issues
}

public sealed class ParentInterviewAction : Action
{
    // Option to schedule and record interview with parent
}

public sealed class CreateSentralEntryAction : Action
{
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
            Result noteAttempt = AddNote($"Incident Number changed from {IncidentNumber} to {incidentNumber} by {currentUser}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;
        }
        else
        {
            Result noteAttempt = AddNote($"Incident Number set to {incidentNumber} by {currentUser}", currentUser);

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
            Result noteAttempt = AddNote($"Not Required set to true with previous Incident Number value of {IncidentNumber} by {currentUser}", currentUser);

            if (noteAttempt.IsFailure)
                return noteAttempt;

            IncidentNumber = 0;
        }
        else
        {
            Result noteAttempt = AddNote($"Not Required set to true by {currentUser}", currentUser);

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
}

public sealed class ConfirmSentralEntryAction : Action
{
    // Option to have user confirm Sentral Incident recorded in the CreateSentralEntryAction was actually created

    private ConfirmSentralEntryAction() 
        : base() { }

    public bool Confirmed { get; private set; }

    public Result Update(bool confirmed, string currentUser)
    {
        Result noteAttempt = AddNote($"Confirmed set to {confirmed} by {currentUser}", currentUser);

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
}

