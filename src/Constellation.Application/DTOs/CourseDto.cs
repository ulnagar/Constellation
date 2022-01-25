using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
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
        public Faculty Faculty { get; set; }
        public Staff HeadTeacher { get; set; }
        [Display(Name=DisplayNameDefaults.HeadTeacherId)]
        public string HeadTeacherId { get; set; }
        [Display(Name=DisplayNameDefaults.FTEValue)]
        public decimal FullTimeEquivalentValue { get; set; }
    }
}