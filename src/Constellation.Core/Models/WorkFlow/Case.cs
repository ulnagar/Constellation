namespace Constellation.Core.Models.WorkFlow;

using Abstractions.Clock;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Case : AggregateRoot, IAuditableEntity
{
    private readonly List<Action> _actions = new();

    private Case() { } // Required by EF Core

    internal Case(
        CaseType type)
    {
        Type = type;

        RaiseDomainEvent(new CaseCreatedDomainEvent(new(), Id));
    }

    public CaseId Id { get; private set; } = new();
    public CaseType? Type { get; private set; }

    public CaseDetailId? DetailId { get; private set; }
    public CaseDetail? Detail { get; private set; }

    public CaseStatus Status { get; private set; } = CaseStatus.Open;

    public IReadOnlyList<Action> Actions => _actions.ToList();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; } = DateTime.MinValue;
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; } = DateTime.MinValue;

    public DateOnly DueDate { get; private set; }

    public void AddAction(Action action)
    {
        _actions.Add(action);

        RaiseDomainEvent(new CaseActionAddedDomainEvent(new(), Id, action.Id));
    }

    public Result AddActionNote(ActionId actionId, string message, string currentUser)
    {
        Action? action = _actions.FirstOrDefault(entry => entry.Id == actionId);

        if (action is null)
            return Result.Failure(CaseErrors.Action.NotFound(actionId));

        Result noteAction = action.AddNote(message, currentUser);

        return noteAction;
    }

    public Result UpdateActionStatus(
        ActionId actionId,
        ActionStatus newStatus,
        string currentUser,
        bool sendEmail = true)
    {
        Action? action = _actions.FirstOrDefault(entry => entry.Id == actionId);

        if (action is null)
            return Result.Failure(CaseErrors.Action.NotFound(actionId));

        if (!action.Status.Equals(ActionStatus.Open))
            return Result.Failure(CaseErrors.Action.UpdateStatus.AlreadyClosed(action.Status));

        Result statusUpdate = action.UpdateStatus(newStatus, currentUser);

        if (newStatus.Equals(ActionStatus.Completed) && sendEmail)
            RaiseDomainEvent(new CaseActionCompletedDomainEvent(new(), Id, actionId));

        if (newStatus.Equals(ActionStatus.Cancelled) && sendEmail)
            RaiseDomainEvent(new CaseActionCancelledDomainEvent(new(), Id, actionId));

        return statusUpdate;
    }

    public Result ReassignAction(
        ActionId actionId,
        Staff newAssignee,
        string currentUser)
    {
        Action? action = _actions.FirstOrDefault(entry => entry.Id == actionId);

        if (action is null)
            return Result.Failure(CaseErrors.Action.NotFound(actionId));

        if (action.AssignedToId == newAssignee.StaffId)
            return Result.Success();

        Result assignmentUpdate = action.AssignAction(newAssignee, currentUser);

        if (assignmentUpdate.IsFailure)
            return assignmentUpdate;

        RaiseDomainEvent(new CaseActionAssignedDomainEvent(new(), Id, action.Id, newAssignee.StaffId));

        return assignmentUpdate;
    }

    public Result UpdateStatus(
        CaseStatus newStatus,
        string currentUser)
    {
        if (newStatus.Equals(CaseStatus.Completed) && _actions.Any(action => action.Status.Equals(ActionStatus.Open)))
            return Result.Failure(CaseErrors.Case.UpdateStatus.CompletedWithOutstandingActions);

        if (newStatus.Equals(CaseStatus.Cancelled))
            foreach (Action action in _actions)
                if (action.Status.Equals(ActionStatus.Open))
                    action.UpdateStatus(ActionStatus.Cancelled, currentUser);

        Status = newStatus;

        return Result.Success();
    }

    public Result AttachDetails(
        CaseDetail detail)
    {
        if (Type!.Equals(CaseType.Attendance))
        {
            if (detail is not AttendanceCaseDetail)
                return Result.Failure(CaseErrors.Case.AttachDetails.DetailMismatch(Type.Name, nameof(AttendanceCaseDetail)));
            
            Detail = detail;
            DetailId = detail.Id;

            return Result.Success();
        }

        return Result.Failure(CaseErrors.Case.AttachDetails.UnknownDetails);
    }

    public Result SetDueDate(IDateTimeProvider dateTime)
    {
        DateOnly expectedDueDate = CreatedAt == DateTime.MinValue 
            ? DateOnly.FromDateTime(dateTime.Now.AddDays(7)) 
            : DateOnly.FromDateTime(CreatedAt.AddDays(7));

        return SetDueDate(dateTime, expectedDueDate);
    }

    public Result SetDueDate(IDateTimeProvider dateTime, DateOnly dueDate)
    {
        if (dueDate < dateTime.Today)
            return Result.Failure(CaseErrors.Case.DueDateInPast(dueDate));

        DueDate = dueDate;

        return Result.Success();
    }

    public override string ToString() => Detail?.ToString() ?? string.Empty;
}