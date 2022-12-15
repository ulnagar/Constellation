namespace Constellation.Core.Models.MandatoryTraining;

using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public class TrainingModule : Entity, IAuditableEntity
{
    public TrainingModule(Guid Id)
        : base(Id)
    {
    }

    public string Name { get; set; } = string.Empty;
    public TrainingModuleExpiryFrequency Expiry { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<TrainingCompletion> Completions { get; set; } = new();
    public bool CanMarkNotRequired { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
