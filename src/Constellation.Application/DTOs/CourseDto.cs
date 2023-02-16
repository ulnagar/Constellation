using Constellation.Application.Helpers;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Grade Grade { get; set; }
        [Required]
        public Guid FacultyId { get; set; }
        public virtual Faculty Faculty { get; set; }
        [Display(Name=DisplayNameDefaults.FTEValue)]
        public decimal FullTimeEquivalentValue { get; set; }
    }
}