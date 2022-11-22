namespace Constellation.Core.Models.MandatoryTraining;

using Constellation.Core.Common;
using Constellation.Core.Enums;
using System;
using System.Collections.Generic;

public class TrainingModule : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TrainingModuleExpiryFrequency Expiry { get; set; }
    public string Url { get; set; }
    public List<TrainingCompletion> Completions { get; set; } = new();
    public bool CanMarkNotRequired { get; set; }

}
