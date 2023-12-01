﻿namespace Constellation.Application.MandatoryTraining.Models;

using Constellation.Core.Models.MandatoryTraining.Identifiers;
using System.Collections.Generic;

public class ModuleDetailsDto
{
    public TrainingModuleId Id { get; set; }
    public string Name { get; set; }
    public string Expiry { get; set; }
    public string Url { get; set; }
    public List<CompletionRecordDto> Completions { get; set; } = new();
    public bool IsActive { get; set; }
}
