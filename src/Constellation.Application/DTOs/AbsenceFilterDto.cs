using Constellation.Application.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class AbsenceFilterDto
    {
        public ReportType Report { get; set; }
        [Display(Name = DisplayNameDefaults.Student)]
        public string StudentId { get; set; }
        [Display(Name = DisplayNameDefaults.DateStart)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }
        [Display(Name = DisplayNameDefaults.DateEnd)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }
        public int? Grade { get; set; }
        [Display(Name = DisplayNameDefaults.School)]
        public string SchoolCode { get; set; }

        public enum ReportType
        {
            [Display(Name = "All Absences")]
            AllAbsences,
            [Display(Name = "No Notifications")]
            NoNotifications,
            [Display(Name = "No Responses")]
            NoResponses
        }
    }
}
