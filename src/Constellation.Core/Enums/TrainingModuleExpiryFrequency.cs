namespace Constellation.Core.Enums;

using System.ComponentModel.DataAnnotations;

public enum TrainingModuleExpiryFrequency
{
    [Display(Name = "Once Off")]
    OnceOff = 0,
    [Display(Name = "Every Year")]
    Annually = 1,
    [Display(Name = "Every 2 Years")]
    TwoYears = 2,
    [Display(Name = "Every 3 Years")]
    ThreeYears = 3,
    [Display(Name = "Every 4 Years")]
    FourYears = 4,
    [Display(Name = "Every 5 Years")]
    FiveYears = 5
}
