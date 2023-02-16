using Constellation.Application.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class LessonDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name=DisplayNameDefaults.DueDate)]
        public DateTime DueDate { get; set; }

        [Required]
        [Display(Name=DisplayNameDefaults.Course)]
        public int CourseId { get; set; }

        [Display(Name=DisplayNameDefaults.GenerateRolls)]
        public bool DoNotGenerateRolls { get; set; }
    }
}
