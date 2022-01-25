using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Periods_UpdateViewModel : BaseViewModel
    {
        public Periods_UpdateViewModel()
        {
            Period = new PeriodDto();
        }

        public PeriodDto Period { get; set; }
        public bool IsNew { get; set; }
        public SelectList TimetableList { get; set; }
        public SelectList TypeList { get; set; }

        public enum ValidDays
        {
            [Display(Name = "Week A - Monday")]
            Monday_Week_A = 1,

            [Display(Name = "Week A - Tuesday")]
            Tuesday_Week_A = 2,

            [Display(Name = "Week A - Wednesday")]
            Wednesday_Week_A = 3,

            [Display(Name = "Week A - Thursday")]
            Thursday_Week_A = 4,

            [Display(Name = "Week A - Friday")]
            Friday_Week_A = 5,
 
            [Display(Name = "Week B - Monday")]
            Monday_Week_B = 6,

            [Display(Name = "Week B - Tuesday")]
            Tuesday_Week_B = 7,

            [Display(Name = "Week B - Wednesday")]
            Wednesday_Week_B = 8,
            
            [Display(Name = "Week B - Thursday")]
            Thursday_Week_B = 9,
            
            [Display(Name = "Week B - Friday")]
            Friday_Week_B = 10
        }
    }
}