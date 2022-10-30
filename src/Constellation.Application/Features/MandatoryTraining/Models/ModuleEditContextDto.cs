using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.MandatoryTraining;
using System;

namespace Constellation.Application.Features.MandatoryTraining.Models;

public class ModuleEditContextDto : IMapFrom<TrainingModule>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Expiry { get; set; }
    public string Url { get; set; }
}
