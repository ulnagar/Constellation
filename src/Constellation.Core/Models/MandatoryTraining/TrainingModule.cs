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

    public string Name { get; set; }
    public TrainingModuleExpiryFrequency Expiry { get; set; }
    public string Url { get; set; }
    public List<TrainingCompletion> Completions { get; set; } = new();
    public bool CanMarkNotRequired { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
