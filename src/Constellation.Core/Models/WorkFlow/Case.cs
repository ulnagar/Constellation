namespace Constellation.Core.Models.WorkFlow;

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

    private Case() { }

    private Case(
        CaseType type)
    {
        Id = new();
        Type = type;
    }

    public CaseId Id { get; private set; }
    public CaseType Type { get; private set; }

    public CaseDetailId DetailId { get; private set; }
    public CaseDetail Detail { get; private set; }

    public IReadOnlyList<Action> Actions => _actions.ToList();
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void AddAction(Action action)
    {
        _actions.Add(action);

        RaiseDomainEvent(new CaseActionAddedDomainEvent(new(), Id, action.Id));
    }

    public Result AddActionNote(ActionId actionId, string message, string currentUser)
    {
        Action action = _actions.FirstOrDefault(entry => entry.Id == actionId);

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
        Action action = _actions.FirstOrDefault(entry => entry.Id == actionId);

        if (action is null)
            return Result.Failure(CaseErrors.Action.NotFound(actionId));

        Result statusUpdate = action.UpdateStatus(newStatus, currentUser);

        return statusUpdate;
    }
}