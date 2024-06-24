using Constellation.Core.Models.Training.Errors;
using Constellation.Core.Shared;
using System.Linq;

namespace Constellation.Core.Models.Training;

using Enums;
using Identifiers;
using Primitives;
using System;
using System.Collections.Generic;

public sealed class TrainingModule : AggregateRoot, IAuditableEntity
{
    private readonly List<TrainingCompletion> _completions = new();
    private readonly List<TrainingModuleAssignee> _assignees = new();

    private TrainingModule(
        string name,
        TrainingModuleExpiryFrequency expiry,
        string url)
    {
        Id = new();
        Name = name;
        Expiry = expiry;
        Url = url;
    }

    public TrainingModuleId Id { get; private set; }
    public string Name { get; private set; }
    public TrainingModuleExpiryFrequency Expiry { get; private set; }
    public string Url { get; private set; }
    public IReadOnlyList<TrainingCompletion> Completions => _completions.AsReadOnly();
    public IReadOnlyList<TrainingModuleAssignee> Assignees => _assignees.AsReadOnly();
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static TrainingModule Create(
        string name,
        TrainingModuleExpiryFrequency expiry,
        string url)
    {
        TrainingModule module = new(
            name,
            expiry,
            url);

        return module;
    }

    public void Update(
        string name,
        TrainingModuleExpiryFrequency expiry,
        string url)
    {
        Name = name;
        Expiry = expiry;
        Url = url;
    }

    public void AddCompletion(TrainingCompletion completion) => _completions.Add(completion);

    public void RemoveCompletion(TrainingCompletion completion) => _completions.Remove(completion);

    public Result AddAssignee(string staffId)
    {
        if (_assignees.Any(entry => entry.StaffId == staffId))
            return Result.Failure(TrainingModuleErrors.AssigneeAlreadyExists(staffId));

        TrainingModuleAssignee assignee = TrainingModuleAssignee.Create(
            Id,
            staffId);

        _assignees.Add(assignee);

        return Result.Success();
    }

    public Result RemoveAssignee(string staffId)
    {
        TrainingModuleAssignee? assignee = _assignees.FirstOrDefault(entry => entry.StaffId == staffId);

        if (assignee is null)
            return Result.Failure(TrainingModuleErrors.AssigneeNotFound(staffId));
        
        _assignees.Remove(assignee);

        return Result.Success();
    }

    public void Delete() => IsDeleted = true;

    public void Reinstate()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = DateTime.MinValue;
    }
}
