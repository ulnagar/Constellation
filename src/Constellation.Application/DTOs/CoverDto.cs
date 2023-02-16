using Constellation.Application.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class CoverDto
    {
        public CoverDto()
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(1);
            SelectedClasses = new List<int>();
        }

        [Display(Name = DisplayNameDefaults.TeacherName)]
        public string UserId { get; set; }
        public string UserType { get; set; }
        [Display(Name = DisplayNameDefaults.ClassName)]
        public int? ClassId { get; set; }
        public IEnumerable<int> SelectedClasses { get; set; }
        public string TeacherId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = DisplayNameDefaults.DateStart)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = DisplayNameDefaults.DateEnd)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
    }
}
