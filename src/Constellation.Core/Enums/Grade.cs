using System.ComponentModel.DataAnnotations;

namespace Constellation.Core.Enums
{
    public enum Grade
    {
        [Display(Name="Year 5")]
        Y05 = 5,
        [Display(Name = "Year 6")]
        Y06 = 6,
        [Display(Name = "Year 7")]
        Y07 = 7,
        [Display(Name = "Year 8")]
        Y08 = 8,
        [Display(Name = "Year 9")]
        Y09 = 9,
        [Display(Name = "Year 10")]
        Y10 = 10,
        [Display(Name = "Year 11")]
        Y11 = 11,
        [Display(Name = "Year 12")]
        Y12 = 12,
        [Display(Name = "Special")]
        SpecialProgram = 13
    }
}