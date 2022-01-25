using Constellation.Presentation.Server.Helpers.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class PeriodDto
    {
        public int? Id { get; set; }
        [Required]
        public int Day { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.PeriodSequence)]
        public int Period { get; set; }
        [Required]
        public string Timetable { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.StartTime)]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.EndTime)]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
        public string Name { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.PeriodType)]
        public string Type { get; set; }
    }
}
