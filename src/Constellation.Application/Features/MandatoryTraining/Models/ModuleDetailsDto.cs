using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.MandatoryTraining.Models;

public class ModuleDetailsDto : IMapFrom<TrainingModule>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Expiry { get; set; }
    public string Url { get; set; }
    public List<CompletionRecordDto> Completions { get; set; } = new();
}
