using System;
using System.Collections.Generic;

namespace Constellation.Core.Models.MandatoryTraining;

public class TrainingModule
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Expiry { get; set; }
    public string Url { get; set; }
    public List<TrainingCompletion> Completions { get; set; } = new();
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
