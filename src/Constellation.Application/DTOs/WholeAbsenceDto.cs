using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class WholeAbsenceDto
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
    }
}