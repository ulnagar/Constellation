using Constellation.Application.Helpers;
using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class CourseOfferingDto
    {
        public OfferingId Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.Course)]
        public int CourseId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name=DisplayNameDefaults.DateStart)]
        public DateOnly StartDate { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name=DisplayNameDefaults.DateEnd)]
        public DateOnly EndDate { get; set; }

        public CourseOfferingDto()
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today);
            EndDate = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 12, 31));
        }
    }
}
