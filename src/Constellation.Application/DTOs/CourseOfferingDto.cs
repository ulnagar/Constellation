using Constellation.Application.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class CourseOfferingDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.Course)]
        public int CourseId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name=DisplayNameDefaults.DateStart)]
        public DateTime StartDate { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name=DisplayNameDefaults.DateEnd)]
        public DateTime EndDate { get; set; }

        public CourseOfferingDto()
        {
            StartDate = DateTime.Now;
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);
        }
    }
}
