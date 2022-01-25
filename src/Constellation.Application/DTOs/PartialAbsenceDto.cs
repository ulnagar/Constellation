using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class PartialAbsenceDto
    {
        public Guid Id { get; set; }

        [Required]
        public string StudentId { get; set; }

        [Required]
        public int OfferingId { get; set; }

        [Required]
        public DateTime Date { get; set; }
        [Required]
        public DateTime DateScanned { get; set; }

        public string PeriodName { get; set; }
        public string PeriodTimeframe { get; set; }

        public int PartialAbsenceLength { get; set; }
        public string PartialAbsenceTimeframe { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Reason { get; set; }
    }
}
