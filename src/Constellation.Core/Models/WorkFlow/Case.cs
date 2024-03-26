#nullable enable
namespace Constellation.Core.Models.WorkFlow;

using Attendance;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using Students;
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
    }

    public CaseId Id { get; private set; } = new();
    public CaseType Type { get; private set; }

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
        string currentUser)
    {
        Action? action = _actions.FirstOrDefault(entry => entry.Id == actionId);

        if (action is null)
            return Result.Failure(CaseErrors.Action.NotFound(actionId));

        Result statusUpdate = action.UpdateStatus(newStatus, currentUser);

        return statusUpdate;
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
        if (Type.Equals(CaseType.Attendance))
        {
            if (detail is not AttendanceCaseDetail)
                return Result.Failure(CaseErrors.Case.AttachDetails.DetailMismatch(Type.Name, nameof(AttendanceCaseDetail)));
            
            Detail = detail;

            return Result.Success();
        }

        return Result.Failure(CaseErrors.Case.AttachDetails.UnknownDetails);
    }
}