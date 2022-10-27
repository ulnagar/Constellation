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
}
