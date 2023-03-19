namespace Constellation.Core.Models.MandatoryTraining;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public class TrainingModule : IAuditableEntity
{
    private readonly List<TrainingCompletion> _completions = new();

    public TrainingModule(TrainingModuleId id)
    {
        Id = id;
    }

    public TrainingModuleId Id { get; private set; }
    public string Name { get; private set; }
    public TrainingModuleExpiryFrequency Expiry { get; private set; }
    public string Url { get; private set; }
    public IReadOnlyList<TrainingCompletion> Completions => _completions;
    public bool CanMarkNotRequired { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
