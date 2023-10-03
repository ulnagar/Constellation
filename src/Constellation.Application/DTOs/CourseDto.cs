using Constellation.Application.Helpers;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class CourseDto
    {
        public CourseId Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(3)]
        public string Code { get; set; }
        [Required]
        public Grade Grade { get; set; }
        [Required]
        public Guid FacultyId { get; set; }
        public virtual Faculty Faculty { get; set; }
        [Display(Name=DisplayNameDefaults.FTEValue)]
        public decimal FullTimeEquivalentValue { get; set; }
    }
}