namespace Constellation.Application.Features.MandatoryTraining.Models;

using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining;
using System;

public class ModuleEditContextDto : IMapFrom<TrainingModule>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TrainingModuleExpiryFrequency Expiry { get; set; }
    public string Url { get; set; }
}
