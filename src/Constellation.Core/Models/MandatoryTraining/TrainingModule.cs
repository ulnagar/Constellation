namespace Constellation.Core.Models.MandatoryTraining;

using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining.Identifiers;
using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrainingModule : IAuditableEntity
{
    private readonly List<TrainingCompletion> _completions = new();
    private readonly List<TrainingRoleModule> _roles = new();

    private TrainingModule(
        TrainingModuleId id,
        string name,
        TrainingModuleExpiryFrequency expiry,
        string url)
    {
        Id = id;
        Name = name;
        Expiry = expiry;
        Url = url;
    }

    public TrainingModuleId Id { get; private set; }
    public string Name { get; private set; }
    public TrainingModuleExpiryFrequency Expiry { get; private set; }
    public string Url { get; private set; }
    public IReadOnlyList<TrainingCompletion> Completions => _completions.ToList();
    public IReadOnlyList<TrainingRoleModule> Roles => _roles.ToList();
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static TrainingModule Create(
        TrainingModuleId id,
        string name,
        TrainingModuleExpiryFrequency expiry,
        string url)
    {
        TrainingModule module = new(
            id,
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

    public void Delete() => IsDeleted = true;

    public void Reinstate()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = DateTime.MinValue;
    }
}
