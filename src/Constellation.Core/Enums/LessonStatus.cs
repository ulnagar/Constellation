using System.ComponentModel.DataAnnotations;

namespace Constellation.Core.Enums
{
    public enum LessonStatus
    {
        [Display(Name = "Unknown")]
        Unknown = 0,
        [Display(Name = "Active")]
        Active = 1,
        [Display(Name = "Completed")]
        Completed = 2,
        [Display(Name = "Cancelled")]
        Cancelled = 3,
        [Display(Name = "Not Applicable")]
        NotApplicable = 4
    }
}
