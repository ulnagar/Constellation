namespace Constellation.Core.Models.WorkFlow;

using Enums;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Action : IAuditableEntity
{
    private readonly List<ActionNote> _notes = new();

    private Action() { }

    public ActionId Id { get; private protected set; } = new();
    public CaseId CaseId { get; private protected set; }
    public ActionStatus Status { get; private protected set; } = ActionStatus.Open;
    public IReadOnlyList<ActionNote> Notes => _notes.ToList();
    
    public string AssignedToId { get; protected set; }
    public string AssignedTo { get; protected set; }
    public DateTime AssignedAt { get; protected set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; internal set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal Result AssignAction(Staff assignee, string currentUser)
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
        string note,
        string currentUser)
    {
        if (newStatus is null)
            return Result.Failure(CaseErrors.Action.UpdateStatus.StatusNull);

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
}

public sealed class CreateSentralEntryAction : Action
{

}

public sealed class ConfirmSentralEntryAction : Action
{

}

