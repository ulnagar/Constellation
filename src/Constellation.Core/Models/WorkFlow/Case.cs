namespace Constellation.Core.Models.WorkFlow;

using Enums;
using Identifiers;
using Primitives;
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


}