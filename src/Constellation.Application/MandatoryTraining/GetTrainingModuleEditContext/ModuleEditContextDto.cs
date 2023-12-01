namespace Constellation.Application.MandatoryTraining.GetTrainingModuleEditContext;

using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public class ModuleEditContextDto
{
    public TrainingModuleId Id { get; set; }
    public string Name { get; set; }
    public TrainingModuleExpiryFrequency Expiry { get; set; }
    public string Url { get; set; }
    public bool CanMarkNotRequired { get; set; }
}
