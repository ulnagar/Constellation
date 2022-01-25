using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class CasualCoverDto
    {
        public int Id { get; set; }

        [Required]
        public int OfferingId { get; set; }

        [Required]
        public int CasualId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsDeleted { get; set; }

    }
}