namespace Constellation.Application.MandatoryTraining.Models;

using Core.Models.Training.Identifiers;

public class ModuleSummaryDto
{
    public TrainingModuleId Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public string Expiry { get; set; }
    public string Url { get; set; }
}
